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
using Windows.UI.Core;

namespace YokeEmulator
{

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Accelerometer accelerometer = null;
        Inclinometer inclinometer = null;
        bool connected = false;

        enum SensorMode { NONE = 0, JOYSTICK = 1, TRACKER = 2 };
        SensorMode mode = SensorMode.NONE;

        int trackPort = 4242;

        bool povTouched = false;
        Ellipse povPoint = null;
        const int povPointSize = 20;

        byte[] isToggle = null;

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
            margin.Left = povCtl.Width / 2 - povPointSize / 2;
            margin.Top = povCtl.Height / 2 - povPointSize / 2;
            povPoint.Margin = margin;
            povPoint.Width = povPointSize;
            povPoint.Height = povPointSize;
            povPoint.Fill = new SolidColorBrush(Colors.Red);
            povPoint.Stroke = new SolidColorBrush(Colors.Yellow);
            povCtl.Children.Add(povPoint);

            inclinometer = Inclinometer.GetDefault();
            inclinometer.ReadingChanged += inclinometer_ReadingChanged;
            accelerometer = Accelerometer.GetDefault();
            accelerometer.ReadingChanged += accelerometer_ReadingChanged;

            App.comHelper.ConnectLose += onLoseConnect;
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
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            int btnCount;
            if (localSettings.Values.ContainsKey("BTNCOUNT"))
            {
                try
                {
                    btnCount = int.Parse(localSettings.Values["BTNCOUNT"].ToString());
                    isToggle = new byte[btnCount];
                    buttonsGridView.Items.Clear();
                    for (int i = 0; i < btnCount; ++i)
                    {

                        string key1 = "BTNLABEL" + i;
                        string key2 = "BTNTOGGLE" + i;
                        if ((bool)localSettings.Values[key2])
                            isToggle[i] = 0;
                        else
                            isToggle[i] = 2;
                        GridViewItem _item = new GridViewItem();
                        _item.Margin = new Thickness(2);
                        _item.Background = new SolidColorBrush(Colors.SkyBlue);
                        _item.Content = localSettings.Values[key1].ToString();
                        _item.FontSize = 20;
                        _item.MinWidth = 80;
                        _item.MinHeight = 60;
                        _item.PointerEntered += buttonsPressed;
                        _item.PointerExited += buttonsReleased;
                        buttonsGridView.Items.Add(_item);
                    }
                }
                catch (Exception)
                {

                }
            }
            else
            {
                btnCount = 0;
                buttonsGridView.Items.Clear();
            }
            if (localSettings.Values.ContainsKey("KEEPSCREENON"))
                if ((bool)localSettings.Values["KEEPSCREENON"])
                {
                    var displayRequest = new Windows.System.Display.DisplayRequest();
                    displayRequest.RequestActive();
                }

            if (localSettings.Values.ContainsKey("TRACKPORT"))
                try
                {
                    trackPort = int.Parse(localSettings.Values["TRACKPORT"].ToString());
                }
                catch (Exception)
                {
                    trackPort = 4242;
                }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("KEEPSCREENON"))
                if ((bool)localSettings.Values["KEEPSCREENON"])
                {
                    var displayRequest = new Windows.System.Display.DisplayRequest();
                    displayRequest.RequestActive();
                }
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
                    App.comHelper.disconnect();
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
                    await App.comHelper.connect(localSettings.Values["IPADDR"].ToString(), trackPort);
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
                    App.comHelper.disconnect();
                }
            }
        }

        void accelerometer_ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            if (connected && mode == SensorMode.JOYSTICK)
                App.comHelper.sendAxis(-e.Reading.AccelerationY * 0.5 + 0.5, -e.Reading.AccelerationX + 0.5);
        }

        async void inclinometer_ReadingChanged(object sender, InclinometerReadingChangedEventArgs e)
        {
            InclinometerReading reading = e.Reading;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (reading.YawAccuracy)
                {
                    case MagnetometerAccuracy.Unknown:
                        sensorStateTextBlock.Text = "Unknown";
                        break;
                    case MagnetometerAccuracy.Unreliable:
                        sensorStateTextBlock.Text = "Unreliable";
                        break;
                    case MagnetometerAccuracy.Approximate:
                        sensorStateTextBlock.Text = "Approximate";
                        break;
                    case MagnetometerAccuracy.High:
                        sensorStateTextBlock.Text = "High";
                        break;
                    default:
                        sensorStateTextBlock.Text = "No data";
                        break;
                }
            });

            if (connected && mode == SensorMode.TRACKER)
                App.comHelper.sendTrack(-reading.YawDegrees, -reading.RollDegrees, -reading.PitchDegrees);
        }

        private async void throttleSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (connected)
                App.comHelper.sendCtl((byte)'t', e.NewValue / 100.0);
        }
        private void rudderSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (connected)
                App.comHelper.sendCtl((byte)'r', e.NewValue / 100.0);
        }

        private void rudderSlider_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            rudderSlider.Value = 50;
            if (connected)
                App.comHelper.sendCtl((byte)'r', 0.5);
        }

        private void povCtl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            povTouched = true;
        }

        private void povCtl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            povTouched = false;
            var margin = povPoint.Margin;
            margin.Left = povCtl.Width / 2 - povPointSize / 2;
            margin.Top = povCtl.Height / 2 - povPointSize / 2;
            povPoint.Margin = margin;


            if (connected)
                App.comHelper.sendCtl((byte)'p', -1);
        }

        private void povCtl_PointerMoved(object sender, PointerRoutedEventArgs e)
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

                App.comHelper.sendCtl((byte)'p',angle);
            }
        }

        private void buttonsPressed(object sender, PointerRoutedEventArgs e)
        {
            int btn = buttonsGridView.Items.IndexOf(sender);
            GridViewItem _item = (GridViewItem)sender;
            if (connected)
            {
                if (isToggle[btn] == 1)
                {
                    App.comHelper.sendCtl((byte)btn, 0);
                    isToggle[btn] = 0;
                    _item.Background = new SolidColorBrush(Colors.SkyBlue);
                }
                else if (isToggle[btn] == 0)
                {
                    App.comHelper.sendCtl((byte)btn, 1);
                    isToggle[btn] = 1;
                    _item.Background = new SolidColorBrush(Colors.OrangeRed);
                }
                else
                {
                    App.comHelper.sendCtl((byte)btn, 1);
                    _item.Background = new SolidColorBrush(Colors.OrangeRed);
                }
            }
        }
        private void buttonsReleased(object sender, PointerRoutedEventArgs e)
        {
            int btn = buttonsGridView.Items.IndexOf(sender);
            if (isToggle[btn] != 2)
                return;
            GridViewItem _item = (GridViewItem)sender;
            _item.Background = new SolidColorBrush(Colors.SkyBlue);
            if (connected)
                App.comHelper.sendCtl((byte)btn, 0);
        }

        private void onLoseConnect()
        {
            connectBtn.Background = new SolidColorBrush(Colors.Red);
            connectBtn.Foreground = new SolidColorBrush(Colors.White);
            connectBtn.Content = "LOSE";
            connected = false;
        }

        private void ModeSwitcher_Click(object sender, PointerRoutedEventArgs e)
        {
            int idxitem = sensorModeGridView.Items.IndexOf(sender);
            if (idxitem == (int)mode)   //Mode not change
                return;
            GridViewItem oldItem = sensorModeGridView.Items[(int)mode] as GridViewItem;
            GridViewItem newItem = sensorModeGridView.Items[idxitem] as GridViewItem;
            Brush oldBrush = oldItem.Background;
            oldItem.Background = newItem.Background;
            newItem.Background = oldBrush;
            mode = (SensorMode)idxitem;
        }
    }
}
