using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Streams;
using Windows.Devices.Sensors;
using Windows.Networking.Sockets;
using Windows.Networking;



// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=391641 上有介绍

namespace YokeEmulator
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Accelerometer accelerometer = null;
        bool connected = false;

        StreamSocket axisSocket = null;
        const int AxisMsgSize = 18;

        StreamSocket ctlSocket = null;
        const int CtlMsgSize = 11;

        bool povTouched = false;
        Ellipse povPoint = null;
        const int povPointSize= 20;
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
            Ellipse bgEllipse = new Ellipse();
            bgEllipse.Width = povCtl.Width;
            bgEllipse.Height = povCtl.Height;
            bgEllipse.Fill = new SolidColorBrush(Colors.SkyBlue);
            povCtl.Children.Add(bgEllipse);

            povPoint = new Ellipse();
            var margin = povPoint.Margin;
            margin.Left = povCtl.Width / 2- povPointSize/2;
            margin.Top = povCtl.Height / 2- povPointSize/2;
            povPoint.Margin = margin;
            povPoint.Width = povPointSize;
            povPoint.Height = povPointSize;
            povPoint.Fill = new SolidColorBrush(Colors.Red);
            povPoint.Stroke = new SolidColorBrush(Colors.Yellow);
            povCtl.Children.Add(povPoint);
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: 准备此处显示的页面。

            // TODO: 如果您的应用程序包含多个页面，请确保
            // 通过注册以下事件来处理硬件“后退”按钮:
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed 事件。
            // 如果使用由某些模板提供的 NavigationHelper，
            // 则系统会为您处理该事件。
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Settings));
        }

        private void CalibrationBtn_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (connected)
            {
                //disconnecting
                try
                {
                    axisSocket.Dispose();
                    ctlSocket.Dispose();
                    connected = false;
                    connectBtn.Foreground = new SolidColorBrush(Colors.Green);
                    connectBtn.Content = connected ? "DisConnect" : "Connect";
                }
                catch (Exception)
                {
                    connectBtn.Foreground = new SolidColorBrush(Colors.Red);
                    connectBtn.Content = "FAILED";
                }
            }
            else
            {
                try
                {
                    //connecting
                    var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    axisSocket = new StreamSocket();
                    ctlSocket = new StreamSocket();
                    HostName hostname = new HostName(localSettings.Values["IPADDR"].ToString());
                    await axisSocket.ConnectAsync(hostname, "23333");
                    await ctlSocket.ConnectAsync(hostname, "23334");

                    connected = true;
                    connectBtn.Foreground = new SolidColorBrush(Colors.Green);
                    connectBtn.Content = connected ? "DisConnect" : "Connect";
                }
                catch (Exception exception)
                {
                    switch (SocketError.GetStatus(exception.HResult))
                    {
                        case SocketErrorStatus.HostNotFound:
                            // Handle HostNotFound Error
                            connectBtn.Foreground = new SolidColorBrush(Colors.Red);
                            connectBtn.Content = "HostNotFound";
                            break;
                        default:
                            // Handle Unknown Error
                            connectBtn.Foreground = new SolidColorBrush(Colors.Red);
                            connectBtn.Content = "FAILED";
                            break;
                    }
                    axisSocket.Dispose();
                    ctlSocket.Dispose();
                }
            }
        }

        private void accelerometerBtn_Click(object sender, RoutedEventArgs e)
        {
            // If the accelerometer is null, it is initialized and started
            if (accelerometer == null)
            {
                // Instantiate the accelerometer sensor object
                accelerometer = Accelerometer.GetDefault();

                // Add an event handler for the ReadingChanged event.
                accelerometer.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(accelerometer_ReadingChanged);

                // The Start method could throw and exception, so use a try block
                accelerometerBtn.Background = new SolidColorBrush(Colors.Green);
                accelerometerBtn.Content = "OFF";
            }
            else
            {
                // if the accelerometer is not null, call Stop
                accelerometer = null;
                accelerometerBtn.Background = new SolidColorBrush(Colors.Red);
                accelerometerBtn.Content = "ON";
            }
        }

        async void accelerometer_ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            if (accelerometer != null)
            {
                if (connected)
                {
                    //byte[] bx = BitConverter.GetBytes(e.Reading.AccelerationX);
                    byte[] by = BitConverter.GetBytes(-e.Reading.AccelerationY * 0.5 + 0.5);
                    byte[] bz = BitConverter.GetBytes(e.Reading.AccelerationZ + 1);

                    byte[] buff = new byte[AxisMsgSize];
                    by.CopyTo(buff, 1);
                    bz.CopyTo(buff, 9);
                    buff[0] = 255;
                    buff[AxisMsgSize-1] = 0;
                    await axisSocket.OutputStream.WriteAsync(buff.AsBuffer());
                }
            }
        }

        private async void throttleSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (connected)
            {
                byte[] param = BitConverter.GetBytes(e.NewValue/100.0);   

                byte[] buff = new byte[CtlMsgSize];
                buff[1] = (byte)'t';
                param.CopyTo(buff, 2);
                buff[0] = 255;
                buff[CtlMsgSize-1] = 0;
                await ctlSocket.OutputStream.WriteAsync(buff.AsBuffer());
            }
        }

        private async void Btn1_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (connected)
            {
                byte[] buff = new byte[CtlMsgSize];
                buff[0] = 255;
                buff[1] = (byte)'b';
                buff[2] = 1;
                buff[3] = 1;
                buff[CtlMsgSize - 1] = 0;
                await ctlSocket.OutputStream.WriteAsync(buff.AsBuffer());
            }
        }

        private async void Btn1_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (connected)
            {
                byte[] buff = new byte[CtlMsgSize];
                buff[0] = 255;
                buff[1] = (byte)'b';
                buff[2] = 1;
                buff[3] = 0;
                buff[CtlMsgSize - 1] = 0;
                await ctlSocket.OutputStream.WriteAsync(buff.AsBuffer());
            }
        }

        private async void Btn2_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (connected)
            {
                byte[] buff = new byte[CtlMsgSize];
                buff[0] = 255;
                buff[1] = (byte)'b';
                buff[2] = 2;
                buff[3] = 1;
                buff[CtlMsgSize - 1] = 0;
                await ctlSocket.OutputStream.WriteAsync(buff.AsBuffer());
            }
        }

        private async void Btn2_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (connected)
            {
                byte[] buff = new byte[CtlMsgSize];
                buff[0] = 255;
                buff[1] = (byte)'b';
                buff[2] = 2;
                buff[3] = 0;
                buff[CtlMsgSize - 1] = 0;
                await ctlSocket.OutputStream.WriteAsync(buff.AsBuffer());
            }
        }

        private async void Btn3_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (connected)
            {
                byte[] buff = new byte[CtlMsgSize];
                buff[0] = 255;
                buff[1] = (byte)'b';
                buff[2] = 3;
                buff[3] = 1;
                buff[CtlMsgSize - 1] = 0;
                await ctlSocket.OutputStream.WriteAsync(buff.AsBuffer());
            }
        }

        private async void Btn3_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (connected)
            {
                byte[] buff = new byte[CtlMsgSize];
                buff[0] = 255;
                buff[1] = (byte)'b';
                buff[2] = 3;
                buff[3] = 0;
                buff[CtlMsgSize - 1] = 0;
                await ctlSocket.OutputStream.WriteAsync(buff.AsBuffer());
            }
        }

        private async void Btn4_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (connected)
            {
                byte[] buff = new byte[CtlMsgSize];
                buff[0] = 255;
                buff[1] = (byte)'b';
                buff[2] = 4;
                buff[3] = 1;
                buff[CtlMsgSize - 1] = 0;
                await ctlSocket.OutputStream.WriteAsync(buff.AsBuffer());
            }
        }

        private async void Btn4_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (connected)
            {
                byte[] buff = new byte[CtlMsgSize];
                buff[0] = 255;
                buff[1] = (byte)'b';
                buff[2] = 4;
                buff[3] = 0;
                buff[CtlMsgSize - 1] = 0;
                await ctlSocket.OutputStream.WriteAsync(buff.AsBuffer());
            }
        }

        private void povCtl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            povTouched = true;
        }

        private async void povCtl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            povTouched = false;
            var margin = povPoint.Margin;
            margin.Left = povCtl.Width / 2 - povPointSize / 2;
            margin.Top = povCtl.Height / 2 - povPointSize / 2;
            povPoint.Margin = margin;
            if (connected)
            {
                byte[] buff = new byte[CtlMsgSize];
                byte[] angle = BitConverter.GetBytes((double)-1);
                buff[0] = 255;
                buff[1] = (byte)'p';
                angle.CopyTo(buff, 2);
                buff[CtlMsgSize - 1] = 0;
                await ctlSocket.OutputStream.WriteAsync(buff.AsBuffer());
            }
        }

        private async void povCtl_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (povTouched && connected)
            {
                var _curpoint = e.GetCurrentPoint(povCtl);
                var margin = povPoint.Margin;
                margin.Left = _curpoint.Position.X - povPointSize / 2;
                margin.Top = _curpoint.Position.Y - povPointSize / 2;
                povPoint.Margin = margin;

                double dx = _curpoint.Position.X - povCtl.Width / 2;
                double dy = _curpoint.Position.Y - povCtl.Height / 2;
                double angle = 180 - Math.Atan2(dx, dy) * 180 / Math.PI;

                byte[] buff = new byte[CtlMsgSize];
                byte[] angleByte = BitConverter.GetBytes(angle);
                buff[0] = 255;
                buff[1] = (byte)'p';
                angleByte.CopyTo(buff, 2);
                buff[CtlMsgSize - 1] = 0;
                await ctlSocket.OutputStream.WriteAsync(buff.AsBuffer());
            }
        }
    }
}
