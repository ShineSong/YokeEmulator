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
    /// YokeEmulator主页面
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

        byte[] isToggle = null; // 0 for release 1 for pressed 2 for only click.

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            //POV椭圆绘制
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
            //传感器初始化
            inclinometer = Inclinometer.GetDefault();
            inclinometer.ReadingChanged += inclinometer_ReadingChanged;
            accelerometer = Accelerometer.GetDefault();
            accelerometer.ReadingChanged += accelerometer_ReadingChanged;
            //连接丢失响应
            App.comHelper.ConnectLose += onLoseConnect;
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
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
        /// <summary>
        /// 跳转前调用
        /// </summary>
        /// <param name="e"></param>
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

        /// <summary>
        /// 跳转到设置页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Settings));
        }

        /// <summary>
        /// 跳转到按钮页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonsPageButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ButtonsPage));
        }

        /// <summary>
        /// 跳转到校准界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalibrationBtn_Click(object sender, RoutedEventArgs e)
        {
            
        }

        /// <summary>
        /// 连接到Server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    if (localSettings.Values.ContainsKey("IPADDR"))
                    {
                        await App.comHelper.connect(localSettings.Values["IPADDR"].ToString(), trackPort);
                        connected = true;
                        connectBtn.Foreground = new SolidColorBrush(Colors.Green);
                        connectBtn.Content = connected ? "DisConnect" : "Connect";
                    }
                    else
                    {
                        connected = true;
                        connectBtn.Content = "NoConfig";
                        connectBtn.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    
                }
                catch (Exception exception)
                {
                    switch (SocketError.GetStatus(exception.HResult))
                    {
                        case SocketErrorStatus.HostNotFound:
                            // Handle HostNotFound Error
                            connectBtn.Background = new SolidColorBrush();
                            connectBtn.Foreground = new SolidColorBrush(Colors.Red);
                            connectBtn.Content = "HostNotFound";
                            break;
                        default:
                            // Handle Unknown Error
                            connectBtn.Background = new SolidColorBrush();
                            connectBtn.Foreground = new SolidColorBrush(Colors.Red);
                            connectBtn.Content = "FAILED";
                            break;
                    }
                    App.comHelper.disconnect();
                }
            }
        }

        /// <summary>
        /// 加速计读数作为摇杆数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void accelerometer_ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            if (connected && mode == SensorMode.JOYSTICK)
                App.comHelper.sendAxis(-e.Reading.AccelerationY * 0.5 + 0.5, -e.Reading.AccelerationX + 0.5);
        }
        /// <summary>
        /// 磁倾仪度数作为头瞄数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// 油门滑块,映射到slider 1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void throttleSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (connected)
                App.comHelper.sendCtl((byte)'t', e.NewValue / 100.0);
        }
        /// <summary>
        /// 脚舵滑块,映射到Z Axis
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rudderSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (connected)
                App.comHelper.sendCtl((byte)'r', e.NewValue / 100.0);
        }
        /// <summary>
        /// 脚舵离手置中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rudderSlider_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            rudderSlider.Value = 50;
            if (connected)
                App.comHelper.sendCtl((byte)'r', 0.5);
        }

        /// <summary>
        /// 苦力帽按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void povCtl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            povTouched = true;
        }

        /// <summary>
        /// 苦力帽松开置中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 计算苦力帽角度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 按钮按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// 按钮松开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// 丢失连接相应
        /// </summary>
        private void onLoseConnect()
        {
            connectBtn.Background = new SolidColorBrush(Colors.Red);
            connectBtn.Foreground = new SolidColorBrush(Colors.White);
            connectBtn.Content = "LOSE";
            connected = false;
        }
        /// <summary>
        /// 传感器模式选择器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
