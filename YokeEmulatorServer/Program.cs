using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vJoyInterfaceWrap;
namespace YokeEmulatorServer
{
    class Program
    {
        static public vJoy joystick;
        static public vJoy.JoystickState iReport;
        static public uint id = 1;

        static long axisMaxval;
        static byte[] axisBuffer = new byte[AxisMsgSize];
        const int AxisPort = 23333;
        const int AxisMsgSize = 18;

        static byte[] ctlBuffer = new byte[CtlMsgSize];
        const int CtlPort = 23334;
        const int CtlMsgSize = 11;
        static bool isPovCon = false;
        static void Main(string[] args)
        {
            try
            {
                joystick = new vJoy();
                iReport = new vJoy.JoystickState();
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("vJoy error,please reinstall vjoy.");
                return;
            }
            
            if (!joystick.vJoyEnabled())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.\n");
                return;
            }
            else
                Console.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n", joystick.GetvJoyManufacturerString(), joystick.GetvJoyProductString(), joystick.GetvJoySerialNumberString());
            // Get the state of the requested device
            VjdStat status = joystick.GetVJDStatus(id);
            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                    Console.WriteLine("vJoy Device {0} is already owned by this feeder\n", id);
                    break;
                case VjdStat.VJD_STAT_FREE:
                    Console.WriteLine("vJoy Device {0} is free\n", id);
                    break;
                case VjdStat.VJD_STAT_BUSY:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("vJoy Device {0} is already owned by another feeder\nCannot continue\n", id);
                    return;
                case VjdStat.VJD_STAT_MISS:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("vJoy Device {0} is not installed or disabled\nCannot continue\n", id);
                    return;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("vJoy Device {0} general error\nCannot continue\n", id);
                    return;
            };
            // Check which axes are supported
            bool AxisX = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_X);
            bool AxisY = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Y);
            bool AxisZ = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Z);
            bool AxisRX = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RX);
            bool AxisRZ = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RZ);
            // Get the number of buttons and POV Hat switchessupported by this vJoy device
            int nButtons = joystick.GetVJDButtonNumber(id);
            int ContPovNumber = joystick.GetVJDContPovNumber(id);
            int DiscPovNumber = joystick.GetVJDDiscPovNumber(id);

            // Print results
            Console.WriteLine("\nvJoy Device {0} capabilities:\n", id);
            Console.WriteLine("Numner of buttons\t\t{0}\n", nButtons);
            Console.WriteLine("Numner of Continuous POVs\t{0}\n", ContPovNumber);
            Console.WriteLine("Numner of Descrete POVs\t\t{0}\n", DiscPovNumber);
            if (ContPovNumber > DiscPovNumber)
                isPovCon = true;
            Console.WriteLine("Axis X\t\t{0}\n", AxisX ? "Yes" : "No");
            Console.WriteLine("Axis Y\t\t{0}\n", AxisX ? "Yes" : "No");
            Console.WriteLine("Axis Z\t\t{0}\n", AxisX ? "Yes" : "No");
            Console.WriteLine("Axis Rx\t\t{0}\n", AxisRX ? "Yes" : "No");
            Console.WriteLine("Axis Rz\t\t{0}\n", AxisRZ ? "Yes" : "No");

            // Test if DLL matches the driver
            UInt32 DllVer = 0, DrvVer = 0;
            bool match = joystick.DriverMatch(ref DllVer, ref DrvVer);
            if (match)
                Console.WriteLine("Version of Driver Matches DLL Version ({0:X})\n", DllVer);
            else
                Console.WriteLine("Version of Driver ({0:X}) does NOT match DLL Version ({1:X})\n", DrvVer, DllVer);

            // Acquire the target
            if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(id))))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to acquire vJoy device number {0}.\n", id);
                return;
            }
            else
                Console.WriteLine("Acquired: vJoy device number {0}.\n", id);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("ALL GREEN.");
            Console.ForegroundColor = ConsoleColor.White;

            joystick.GetVJDAxisMax(id, HID_USAGES.HID_USAGE_X, ref axisMaxval);


            try
            {
                TcpListener axisTcp;
                axisTcp = new TcpListener(IPAddress.Any, AxisPort);
                axisTcp.Start(1);
                axisTcp.BeginAcceptSocket(axisClientConnect, axisTcp);

                TcpListener ctlTcp;
                ctlTcp = new TcpListener(IPAddress.Any, CtlPort);
                ctlTcp.Start(1);
                ctlTcp.BeginAcceptSocket(ctlClientConnect, ctlTcp);
                Console.WriteLine("started listening...");

                IPAddress[] arrIPAddresses = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (IPAddress ip in arrIPAddresses)
                {
                    if (ip.AddressFamily.Equals(AddressFamily.InterNetwork))
                    {
                        Console.WriteLine(ip.ToString());
                    }
                }
            }
            catch (System.Security.SecurityException)
            {
                Console.WriteLine("firewall banned application.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            while (true) Thread.Sleep(1000000);
        }

        static void axisClientConnect(IAsyncResult ar)
        {
            TcpListener ServerSocket = (TcpListener)ar.AsyncState;
            Socket ClientSocket = ServerSocket.EndAcceptSocket(ar);
            Console.WriteLine("Axis Channel Client Connected.");
            joystick.ResetVJD(id);

            ClientSocket.BeginReceive(axisBuffer, 0, AxisMsgSize, SocketFlags.None, new AsyncCallback(axisRead), ClientSocket);
            ServerSocket.BeginAcceptSocket(new AsyncCallback(axisClientConnect), ServerSocket);
        }

        static void ctlClientConnect(IAsyncResult ar)
        {
            TcpListener ServerSocket = (TcpListener)ar.AsyncState;
            Socket ClientSocket = ServerSocket.EndAcceptSocket(ar);
            Console.WriteLine("Ctrl Channel Client Connected.");
            joystick.ResetVJD(id);

            ClientSocket.BeginReceive(ctlBuffer, 0, CtlMsgSize, SocketFlags.None, new AsyncCallback(ctlRead), ClientSocket);
            ServerSocket.BeginAcceptSocket(new AsyncCallback(ctlClientConnect), ServerSocket);
        }

        static void axisRead(IAsyncResult ar)
        {
            Socket SocketClient = (Socket)ar.AsyncState;
            int ByteRead = 0;
            try
            {
                ByteRead = SocketClient.EndReceive(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine("Axis Channel Disconnect.");
                return;
            }
            if (ByteRead == 0)
            {
                Console.WriteLine("Axis Channel Disconnect.");
                return;
            }
            else if (ByteRead < AxisMsgSize)
            {
                Console.WriteLine("Axis Channel Error Connect.");
                Console.Beep();
                return;
            }

            byte veri1 = axisBuffer[0];
            byte veri2 = axisBuffer[AxisMsgSize - 1];

            if (veri1 != 255 || veri2 != 0)
            {
                Console.WriteLine("Axis Channel Verify Error.");
                Console.Beep();
                return;
            }

            double x = BitConverter.ToDouble(axisBuffer, 1) * axisMaxval;
            double y = BitConverter.ToDouble(axisBuffer, 9) * axisMaxval;
            if (x < 0) x = 0; else if (x > axisMaxval) x = axisMaxval;
            if (y < 0) y = 0; else if (y > axisMaxval) y = axisMaxval;
            joystick.SetAxis((int)x, id, HID_USAGES.HID_USAGE_X);
            joystick.SetAxis((int)y, id, HID_USAGES.HID_USAGE_Y);

            SocketClient.BeginReceive(axisBuffer, 0, AxisMsgSize, SocketFlags.None, new AsyncCallback(axisRead), SocketClient);
        }

        static void ctlRead(IAsyncResult ar)
        {
            Socket SocketClient = (Socket)ar.AsyncState;
            int ByteRead = 0;
            try
            {
                ByteRead = SocketClient.EndReceive(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine("Ctrl Channel Disconnect.");
                return;
            }
            if (ByteRead == 0)
            {
                Console.WriteLine("Ctrl Channel Disconnect.");
                return;
            }
            else if (ByteRead < CtlMsgSize)
            {
                Console.WriteLine("Ctrl Channel Error Connect.");
                Console.Beep();
                return;
            }

            byte veri1 = ctlBuffer[0];
            byte veri2 = ctlBuffer[CtlMsgSize - 1];

            if (veri1 != 255 || veri2 != 0)
            {
                Console.WriteLine("Ctrl Channel Verify Error.");
                Console.Beep();
                return;
            }
            byte command = ctlBuffer[1];
            switch (command)
            {
                case (byte)'t':
                    double throttle = BitConverter.ToDouble(ctlBuffer, 2);
                    joystick.SetAxis((int)(throttle * axisMaxval), id, HID_USAGES.HID_USAGE_SL0);
                    break;
                case (byte)'r':
                    double rudder = BitConverter.ToDouble(ctlBuffer, 2);
                    joystick.SetAxis((int)(rudder * axisMaxval), id, HID_USAGES.HID_USAGE_Z);
                    break;
                case (byte)'b':
                    byte bid = ctlBuffer[2];
                    byte state = ctlBuffer[3];
                    joystick.SetBtn(state == 1, id, bid);
                    break;
                case (byte)'p':
                    double pov = BitConverter.ToDouble(ctlBuffer, 2);
                    if (pov > 0)
                        if (isPovCon)
                            joystick.SetContPov((int)pov * 100, id, 1);
                        else
                        {
                            int ori = 0;
                            if (pov > 315) ori = 0;
                            else if (pov > 225) ori = 3;
                            else if (pov > 135) ori = 2;
                            else if (pov > 45) ori = 1;
                            else ori = 0;
                            joystick.SetDiscPov(ori, id, 1);
                        }
                    else
                        joystick.SetContPov(-1, id, 1);
                    break;
                default:
                    Console.WriteLine("Ctrl command unknown.");
                    break;
            }

            SocketClient.BeginReceive(ctlBuffer, 0, CtlMsgSize, SocketFlags.None, new AsyncCallback(ctlRead), SocketClient);
        }
    }

}

