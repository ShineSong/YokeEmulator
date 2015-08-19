using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Networking;
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

        

        public bool connected = false;

        int trackPort = 4242;
        string host = null;
        int reconnectCount = 0;
        public async void connect(string _host, int _trackPort)
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

            reconnectCount = 0;
            connected = true;
        }

        public void disconnect()
        {
            axisSocket.Dispose();
            ctlSocket.Dispose();
            trackSocket.Dispose();
            reconnectCount = 0;
            connected = false;
        }

        public void reconnect()
        {
            if (reconnectCount > 3)
                return;
            connect(host, trackPort);
        }

        public async void sendAxis(double x, double y)
        {

        }

        public async void sendCtl(char op, double param)
        {

        }

        public async void sendCtl(char op, char bid, char state)
        {

        }

        public async void sendTrack(double yaw, double pitch, double roll)
        {

        }

        void onLoseConnect()
        {

        }
    }
}
