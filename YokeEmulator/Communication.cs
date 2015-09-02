using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Networking;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;

namespace YokeEmulator
{
    /// <summary>
    /// Communication class and operation.
    /// </summary>
    public class Communication
    {
        DatagramSocket axisSocket = null;
        const int AxisMsgSize = 16;
        byte[] AxisBuffer;
        DatagramSocket ctlSocket = null;
        const int CtlMsgSize = 9;
        byte[] CtlBuffer;
        DatagramSocket trackSocket = null;
        const int trackMsgSize = 48;
        byte[] trackBuffer;
        public double _trackZ;

        public bool connected = false;
        int trackPort = 4242;
        string host = null;

        /// <summary>
        /// connect to server
        /// </summary>
        /// <param name="_host">target ip address</param>
        /// <param name="_axisPort">axis channel port</param>
        /// <param name="_ctlPort">control channel port</param>
        /// <param name="_trackPort">track channel port</param>
        /// <returns></returns>
        public async Task connect(string _host,int _axisPort,int _ctlPort, int _trackPort)
        {
            host = _host; trackPort = _trackPort;
            AxisBuffer = new byte[AxisMsgSize];
            CtlBuffer = new byte[CtlMsgSize];
            trackBuffer = new byte[trackMsgSize];
            //connecting
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            axisSocket = new DatagramSocket();
            ctlSocket = new DatagramSocket();
            trackSocket = new DatagramSocket();
            HostName hostname = new HostName(host);
            await axisSocket.ConnectAsync(hostname, _axisPort.ToString());
            await ctlSocket.ConnectAsync(hostname, _ctlPort.ToString());
            await trackSocket.ConnectAsync(hostname, trackPort.ToString());
            connected = true;
        }

        /// <summary>
        /// cut connection down
        /// </summary>
        public void disconnect()
        {
            connected = false;
            axisSocket.Dispose();
            ctlSocket.Dispose();
            trackSocket.Dispose();
        }

        /// <summary>
        /// 发送摇杆数据
        /// </summary>
        /// <param name="x">X轴(0~1)</param>
        /// <param name="y">Y轴(0~1)</param>
        public async void sendAxis(double x, double y)
        {
            byte[] bx = BitConverter.GetBytes(x);
            byte[] by = BitConverter.GetBytes(y);

            bx.CopyTo(AxisBuffer, 0);
            by.CopyTo(AxisBuffer, 8);
            await axisSocket.OutputStream.WriteAsync(AxisBuffer.AsBuffer());
        }
        /// <summary>
        /// 发送控制数据
        /// </summary>
        /// <param name="op">操作符</param>
        /// <param name="param">参数</param>
        public void sendCtl(byte op, double param)
        {
            byte[] paramByte = BitConverter.GetBytes(param);
            CtlBuffer[0] = op;
            paramByte.CopyTo(CtlBuffer, 1);
            ctlSocket.OutputStream.WriteAsync(CtlBuffer.AsBuffer());
        }
        /// <summary>
        /// 发送按钮数据
        /// </summary>
        /// <param name="bid">按钮ID</param>
        /// <param name="state">状态(0 for release,1 for press)</param>
        public void sendCtl(byte bid, byte state)
        {
            CtlBuffer[0] = (byte)'b';
            CtlBuffer[1] = (byte)bid;
            CtlBuffer[2] = (byte)state;
            ctlSocket.OutputStream.WriteAsync(CtlBuffer.AsBuffer());
        }
        /// <summary>
        /// 发送头瞄数据
        /// </summary>
        /// <param name="yaw">yaw</param>
        /// <param name="pitch">pitch</param>
        /// <param name="roll">roll</param>
        public void sendTrack(double yaw, double pitch, double roll)
        {
            byte[] tz = BitConverter.GetBytes(_trackZ);
            byte[] rx = BitConverter.GetBytes(yaw);
            byte[] ry = BitConverter.GetBytes(pitch);
            byte[] rz = BitConverter.GetBytes(roll);
            tz.CopyTo(trackBuffer, 16);
            rx.CopyTo(trackBuffer, 24);
            ry.CopyTo(trackBuffer, 32);
            rz.CopyTo(trackBuffer, 40);
            trackSocket.OutputStream.WriteAsync(trackBuffer.AsBuffer());
        }
    }
}
