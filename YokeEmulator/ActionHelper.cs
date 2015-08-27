using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using Windows.Networking.Sockets;

namespace YokeEmulator
{
    public class ActionHelper
    {
        Accelerometer accelerometer = null;
        Inclinometer inclinometer = null;

        public bool connected = false;
        public MagnetometerAccuracy InclinometerState;
        public event EventHandler InclinometerStateChanged;
        public enum SensorMode { NONE = 0, JOYSTICK = 1, TRACKER = 2 };
        public SensorMode mode = SensorMode.NONE;

        public ActionHelper()
        {
            inclinometer = Inclinometer.GetDefault();
            inclinometer.ReadingChanged += inclinometer_ReadingChanged;
            accelerometer = Accelerometer.GetDefault();
            accelerometer.ReadingChanged += accelerometer_ReadingChanged;
            InclinometerState = MagnetometerAccuracy.Unknown;
        }

        public async Task connectTo(string ipaddr, int trackPort)
        {
            if (connected)
            {
                //disconnecting
                App.comHelper.disconnect();
                connected = false;
            }
            else
            {
                try
                {
                    //connecting
                    await App.comHelper.connect(ipaddr, trackPort);
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

        private void onLoseConnect()
        {
            throw new NotImplementedException();
        }
        public void OnSliderValueChanged(double value)
        {
            if (connected)
                App.comHelper.sendCtl((byte)'t', value / 100.0);
        }
        public void OnRudderValueChanged(double value)
        {
            if (connected)
                App.comHelper.sendCtl((byte)'r', value / 100.0);
        }
        public void OnButtonPressed(int bid)
        {
            if (connected)
                App.comHelper.sendCtl((byte)bid, 1);
        }

        public void OnButtonReleased(int bid)
        {
            if (connected)
                App.comHelper.sendCtl((byte)bid, 0);
        }

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
                App.comHelper.sendTrack(-reading.YawDegrees, -reading.RollDegrees, -reading.PitchDegrees);
        }

        private void accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
            if (connected && mode == SensorMode.JOYSTICK)
                App.comHelper.sendAxis(-args.Reading.AccelerationY * 0.5 + 0.5, -args.Reading.AccelerationX + 0.5);
        }
    }
}
