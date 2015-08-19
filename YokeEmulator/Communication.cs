using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Networking;
using System.Runtime.InteropServices.WindowsRuntime;

namespace YokeEmulator
{
    /// <summary>
    /// 通讯对象及函数封装类
    /// </summary>
    public class Communication
    {
        StreamSocket axisSocket = null;
        const int AxisMsgSize = 18;
        StreamSocket ctlSocket = null;
        const int CtlMsgSize = 11;
        DatagramSocket trackSocket = null;
        const int trackMsgSize = 48;

        public delegate void ConnectLoseHandler();
        public event ConnectLoseHandler ConnectLose;


        public bool connected = false;
        int trackPort = 4242;
        string host = null;

        /// <summary>
        /// 连接到Server.可能抛出异常.
        /// </summary>
        /// <param name="_host">目标地址</param>
        /// <param name="_trackPort">Track UDP 端口</param>
        /// <returns></returns>
        public async Task connect(string _host, int _trackPort)
        {
            host = _host; trackPort = _trackPort;
            //connecting
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            axisSocket = new StreamSocket();
            ctlSocket = new StreamSocket();
            trackSocket = new DatagramSocket();
            HostName hostname = new HostName(host);
            await axisSocket.ConnectAsync(hostname, "23333");
            await ctlSocket.ConnectAsync(hostname, "23334");
            await trackSocket.ConnectAsync(hostname, trackPort.ToString());
            connected = true;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void disconnect()
        {
            axisSocket.Dispose();
            ctlSocket.Dispose();
            trackSocket.Dispose();
            connected = false;
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

            byte[] buff = new byte[AxisMsgSize];
            bx.CopyTo(buff, 1);
            by.CopyTo(buff, 9);
            buff[0] = 255;
            buff[AxisMsgSize - 1] = 0;
            try
            {
                await axisSocket.OutputStream.WriteAsync(buff.AsBuffer());
            }
            catch
            {
                onLoseConnect();
            }
        }
        /// <summary>
        /// 发送控制数据
        /// </summary>
        /// <param name="op">操作符</param>
        /// <param name="param">参数</param>
        public async void sendCtl(byte op, double param)
        {
            byte[] buff = new byte[CtlMsgSize];
            byte[] paramByte = BitConverter.GetBytes(param);
            buff[0] = 255;
            buff[1] = op;
            paramByte.CopyTo(buff, 2);
            buff[CtlMsgSize - 1] = 0;
            try
            {
                await ctlSocket.OutputStream.WriteAsync(buff.AsBuffer());
            }
            catch
            {
                onLoseConnect();
            }
        }
        /// <summary>
        /// 发送按钮数据
        /// </summary>
        /// <param name="bid">按钮ID</param>
        /// <param name="state">状态(0 for release,1 for press)</param>
        public async void sendCtl(byte bid, byte state)
        {
            byte[] buff = new byte[CtlMsgSize];
            buff[0] = 255;
            buff[1] = (byte)'b';
            buff[2] = (byte)(bid + 1);
            buff[3] = (byte)state;
            buff[CtlMsgSize - 1] = 0;
            try
            {
                await ctlSocket.OutputStream.WriteAsync(buff.AsBuffer());
            }
            catch
            {
                onLoseConnect();
            }
        }
        /// <summary>
        /// 发送头瞄数据
        /// </summary>
        /// <param name="yaw">yaw</param>
        /// <param name="pitch">pitch</param>
        /// <param name="roll">roll</param>
        public async void sendTrack(double yaw, double pitch, double roll)
        {
            byte[] rx = BitConverter.GetBytes(yaw);
            byte[] ry = BitConverter.GetBytes(pitch);
            byte[] rz = BitConverter.GetBytes(roll);
            byte[] buff = new byte[trackMsgSize];
            rx.CopyTo(buff, 24);
            ry.CopyTo(buff, 32);
            rz.CopyTo(buff, 40);
            try
            {
                await trackSocket.OutputStream.WriteAsync(buff.AsBuffer());
            }
            catch
            {
                onLoseConnect();
            }
        }
        /// <summary>
        /// 连接丢失时三次重拨,仍连接不上啧激活ConnectLose事件.
        /// </summary>
        async void onLoseConnect()
        {
            int reConnectCount = 0;
            while (reConnectCount < 3)
            {
                try { await connect(host, trackPort); } catch (Exception) { reConnectCount++; }
            }
            disconnect();
            ConnectLose(); //emit ConnectLose Event
        }
    }
}
