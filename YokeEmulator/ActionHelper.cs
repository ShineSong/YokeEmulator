using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using Windows.Networking.Sockets;

namespace YokeEmulator
{
    /// <summary>
    /// This is the core class of the app. Packaging sensor relate operations and net io in the class.
    /// </summary>
    public class ActionHelper
    {
        Inclinometer inclinometer = null;

        public bool connected = false;
        public MagnetometerAccuracy InclinometerState;
        public event EventHandler InclinometerStateChanged;
        public enum SensorMode { NONE = 0, JOYSTICK = 1, TRACKER = 2,CALIBRATION = 3 };
        public SensorMode mode = SensorMode.NONE;
        public double offYaw, offRoll, offPitch;
        public ActionHelper()
        {
            inclinometer = Inclinometer.GetDefault();
            inclinometer.ReadingChanged += inclinometer_ReadingChanged;
            InclinometerState = MagnetometerAccuracy.Unknown;
            offYaw = 0;offRoll = 0;offPitch = 0;
        }

        /// <summary>
        /// connect to server
        /// </summary>
        /// <param name="ipaddr">target ip address</param>
        /// <param name="axisPort">axis channel port</param>
        /// <param name="ctlPort">control channel port</param>
        /// <param name="trackPort">track channel port</param>
        /// <returns></returns>
        public async Task connectTo(string ipaddr,int axisPort,int ctlPort, int trackPort)
        {
            if (connected)
            {
                //disconnecting
                connected = false;
                App.comHelper.disconnect();
            }
            else
            {
                try
                {
                    //connecting
                    await App.comHelper.connect(ipaddr,axisPort, ctlPort, trackPort);
                    connected = true;
                    mode = SensorMode.JOYSTICK;
                }
                catch (Exception exception)
                {
                    connected = false;
                    App.comHelper.disconnect();
                    throw exception;
                }
            }
        }
        
        /// <summary>
        /// send throttle value to axis-slider-0
        /// </summary>
        /// <param name="value"></param>
        public void OnSliderValueChanged(double value)
        {
            if (connected)
                App.comHelper.sendCtl((byte)'s', value / 100.0);
        }
        /// <summary>
        /// send rudder value to axis-z
        /// </summary>
        /// <param name="value"></param>
        public void OnRudderValueChanged(double value)
        {
            if (connected)
                App.comHelper.sendCtl((byte)'z', value / 100.0);
        }
        /// <summary>
        /// send button pressed event
        /// </summary>
        /// <param name="bid"></param>
        public void OnButtonPressed(int bid)
        {
            if (connected)
                App.comHelper.sendCtl((byte)bid, 1);
        }
        /// <summary>
        /// send button released event
        /// </summary>
        /// <param name="bid"></param>
        public void OnButtonReleased(int bid)
        {
            if (connected)
                App.comHelper.sendCtl((byte)bid, 0);
        }
        /// <summary>
        /// send sensor data to server ,depend on different mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void inclinometer_ReadingChanged(Inclinometer sender, InclinometerReadingChangedEventArgs args)
        {
            InclinometerReading reading = args.Reading;

            if (args.Reading.YawAccuracy != InclinometerState)
            {
                InclinometerState = args.Reading.YawAccuracy;
                if (InclinometerStateChanged != null)
                    InclinometerStateChanged(this, EventArgs.Empty);
            }

            if (connected && mode == SensorMode.TRACKER)
                App.comHelper.sendTrack(-(reading.YawDegrees-offYaw), -(reading.RollDegrees-offRoll), -(reading.PitchDegrees-offPitch));
            else if (connected && mode == SensorMode.JOYSTICK)
                App.comHelper.sendAxis((reading.PitchDegrees-offPitch) / 120 + 0.5, -(reading.RollDegrees-offRoll) / 100 + 0.5);
            else if (mode == SensorMode.CALIBRATION)
            {
                offYaw = reading.YawDegrees;
                offRoll = reading.RollDegrees;
                offPitch = reading.PitchDegrees;
            }
        }
    }
}
