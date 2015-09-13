using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using vJoyInterfaceWrap;

namespace YokeEmulatorServer
{
    public class UdpState
    {
        public UdpClient udpClient;
        public IPEndPoint ipEndPoint;
    }
    class Program
    {
        const string version = "1.1.0.1";
        static public vJoy joystick;
        static public vJoy.JoystickState iReport;
        static public uint id = 1;

        static long axisMaxval;

        static byte[] AxisBuffer = new byte[AxisUDPMsgSize];
        const int AxisPort = 23333;
        const int AxisUDPMsgSize = 16;

        static byte[] CtlBuffer = new byte[CtlUDPMsgSize];
        const int CtlPort = 23334;
        const int CtlUDPMsgSize = 9;

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
            bool AxisRY = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RX);
            bool AxisRZ = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RZ);
            bool AxisSL0 = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_SL0);
            bool AxisSL1 = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_SL1);
            // Get the number of buttons and POV Hat switchessupported by this vJoy device
            int nButtons = joystick.GetVJDButtonNumber(id);
            int ContPovNumber = joystick.GetVJDContPovNumber(id);
            int DiscPovNumber = joystick.GetVJDDiscPovNumber(id);

            // Print results
            Console.WriteLine("vJoy Device {0}", id);
            Console.WriteLine("Checking...Numner of buttons\t\t{0}\tPass", nButtons);
            Console.WriteLine("Checking...Numner of Continuous POVs\t{0}\tPass", ContPovNumber);
            Console.WriteLine("Checking...Numner of Descrete POVs\t{0}\tPass", DiscPovNumber);
            if (ContPovNumber > DiscPovNumber)
                isPovCon = true;
            Console.WriteLine("Checking...Axis X\t\t\t{0}\tPass", AxisX ? "Yes" : "No");
            Console.WriteLine("Checking...Axis Y\t\t\t{0}\tPass", AxisX ? "Yes" : "No");
            Console.WriteLine("Checking...Axis Z\t\t\t{0}\tPass", AxisX ? "Yes" : "No");
            Console.WriteLine("Checking...Axis Rx\t\t\t{0}\tPass", AxisRX ? "Yes" : "No");
            Console.WriteLine("Checking...Axis Ry\t\t\t{0}\tPass", AxisRY ? "Yes" : "No");
            Console.WriteLine("Checking...Axis Rz\t\t\t{0}\tPass", AxisRZ ? "Yes" : "No");
            Console.WriteLine("Checking...Axis SL0\t\t\t{0}\tPass", AxisSL0 ? "Yes" : "No");
            Console.WriteLine("Checking...Axis SL1\t\t\t{0}\tPass", AxisSL1 ? "Yes" : "No");
                                                           
            // Test if DLL matches the driver
            UInt32 DllVer = 0, DrvVer = 0;
            bool match = joystick.DriverMatch(ref DllVer, ref DrvVer);
            if (match)
                Console.WriteLine("Checking...Version of Driver Matches DLL Version ({0:X})\n", DllVer);
            else
                Console.WriteLine("Checking...Version of Driver ({0:X}) does NOT match DLL Version ({1:X})\n", DrvVer, DllVer);

            // Acquire the target
            if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(id))))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to acquire vJoy device number {0}.\n", id);
                return;
            }
            else
                Console.WriteLine("Acquired: vJoy device number {0}.\n", id);

            joystick.ResetAll();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("System All Green.Now Start...");
            Console.ForegroundColor = ConsoleColor.White;

            readConfig();
            printHello();

            joystick.GetVJDAxisMax(id, HID_USAGES.HID_USAGE_X, ref axisMaxval);


            UdpState axisUdpState = new UdpState();
            axisUdpState.ipEndPoint = new IPEndPoint(IPAddress.Any, AxisPort);
            axisUdpState.udpClient = new UdpClient(AxisPort);
            axisUdpState.udpClient.BeginReceive(AxisUDPRecieved, axisUdpState);

            UdpState ctlUdpState = new UdpState();
            ctlUdpState.ipEndPoint = new IPEndPoint(IPAddress.Any, CtlPort);
            ctlUdpState.udpClient = new UdpClient(CtlPort);
            ctlUdpState.udpClient.BeginReceive(CtlUDPRecieved, ctlUdpState);

            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in nics)
                {
                    if (adapter.OperationalStatus != OperationalStatus.Up) //filter unavaliable interface
                        continue;
                    if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet | adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    {
                        IPInterfaceProperties ip = adapter.GetIPProperties();
                        UnicastIPAddressInformationCollection ipCollection = ip.UnicastAddresses;
                        foreach (UnicastIPAddressInformation ipadd in ipCollection)
                        {
                            if (ipadd.Address.AddressFamily == AddressFamily.InterNetwork)
                                //判断是否为ipv4
                                Console.WriteLine(adapter.Name + " : " + ipadd.Address.ToString());//获取ip
                        }
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
        static void printHello()
        {
            Console.WriteLine("============ YokeEmulator {0} ============", version);
            Console.WriteLine("Copyright (C) 2015  SxS (shinesong_sxs@foxmail.com)");
            Console.WriteLine("Start Listenning on {0} for Axis Channel and {1} for Command Channel\n",AxisPort,CtlPort);
        }

        static void readConfig()
        {
            ConfigurationFileMap fileMap = new ConfigurationFileMap(System.Environment.CurrentDirectory + "\\Emulator.config");
            Configuration a = ConfigurationManager.OpenMappedMachineConfiguration(fileMap);
        }

        static void AxisUDPRecieved(IAsyncResult ar)
        {
            UdpState state = (UdpState)ar.AsyncState;
            AxisBuffer = state.udpClient.EndReceive(ar, ref state.ipEndPoint);
            if (AxisBuffer.Length != AxisUDPMsgSize)
            {
                Console.WriteLine("AXIS ERR "+ AxisBuffer.Length.ToString());
                Console.WriteLine(state.ipEndPoint.Port);
            }
            double rawx = BitConverter.ToDouble(AxisBuffer, 0);
            double rawy = BitConverter.ToDouble(AxisBuffer, 8);
            joystick.SetAxis((int)(rawx * axisMaxval), id, HID_USAGES.HID_USAGE_X);
            joystick.SetAxis((int)(rawy * axisMaxval), id, HID_USAGES.HID_USAGE_Y);
            state.udpClient.BeginReceive(AxisUDPRecieved, state);
        }
        static void CtlUDPRecieved(IAsyncResult ar)
        {
            UdpState state = (UdpState)ar.AsyncState;
            CtlBuffer = state.udpClient.EndReceive(ar, ref state.ipEndPoint);
            if (CtlBuffer.Length != CtlUDPMsgSize)
            {
                Console.WriteLine("CTL ERR " + CtlBuffer.Length.ToString());
            }

            byte command = CtlBuffer[0];
            double val;
            byte bid, bstate;
            switch (command)
            {
                case (byte)'s':
                    val = BitConverter.ToDouble(CtlBuffer, 1);
                    joystick.SetAxis((int)(val * axisMaxval), id, HID_USAGES.HID_USAGE_SL0);
                    break;
                case (byte)'l':
                    val = BitConverter.ToDouble(CtlBuffer, 1);
                    joystick.SetAxis((int)(val * axisMaxval), id, HID_USAGES.HID_USAGE_SL1);
                    break;
                case (byte)'x':
                    val = BitConverter.ToDouble(CtlBuffer, 1);
                    joystick.SetAxis((int)(val * axisMaxval), id, HID_USAGES.HID_USAGE_X);
                    break;
                case (byte)'y':
                    val = BitConverter.ToDouble(CtlBuffer, 1);
                    joystick.SetAxis((int)(val * axisMaxval), id, HID_USAGES.HID_USAGE_Y);
                    break;
                case (byte)'z':
                    val = BitConverter.ToDouble(CtlBuffer, 1);
                    joystick.SetAxis((int)(val * axisMaxval), id, HID_USAGES.HID_USAGE_Z);
                    break;
                case (byte)'X':
                    val = BitConverter.ToDouble(CtlBuffer, 1);
                    joystick.SetAxis((int)(val * axisMaxval), id, HID_USAGES.HID_USAGE_RX);
                    break;
                case (byte)'Y':
                    val = BitConverter.ToDouble(CtlBuffer, 1);
                    joystick.SetAxis((int)(val * axisMaxval), id, HID_USAGES.HID_USAGE_RY);
                    break;
                case (byte)'Z':
                    val = BitConverter.ToDouble(CtlBuffer, 1);
                    joystick.SetAxis((int)(val * axisMaxval), id, HID_USAGES.HID_USAGE_RZ);
                    break;
                case (byte)'b':
                    bid = CtlBuffer[1];
                    bstate = CtlBuffer[2];
                    joystick.SetBtn(bstate == 1, id, bid);
                    break;
                case (byte)'p':
                    double pov = BitConverter.ToDouble(CtlBuffer, 1);
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
            state.udpClient.BeginReceive(CtlUDPRecieved, state);
        }
    }
}

