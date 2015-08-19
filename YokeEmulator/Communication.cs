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
        public async Task connect(string _host, int _trackPort)
        {
            host = _host;trackPort = _trackPort;
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

        public void disconnect()
        {
            axisSocket.Dispose();
            ctlSocket.Dispose();
            trackSocket.Dispose();
            connected = false;
        }

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

        async void onLoseConnect()
        {
            int reConnectCount = 0;
            while (reConnectCount < 3)
            {
                try { await connect(host, trackPort); } catch (Exception) { reConnectCount++; }
            }
            ConnectLose(); //emit ConnectLose Event
        }
    }
}
