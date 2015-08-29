using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using YokeCtl;
// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace YokeEmulator
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FlyPage : Page
    {
        bool leftTrackBtnPressed;
        bool rightTrackBtnPressed;
        public FlyPage()
        {
            this.InitializeComponent();
            leftTrackBtnPressed = false;
            rightTrackBtnPressed = false;
            App.actionHelper.InclinometerStateChanged += ActionHelper_InclinometerStateChanged;
            App.comHelper.ConnectLose += ComHelper_ConnectLose;
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("KEEPSCREENON"))
                if ((bool)localSettings.Values["KEEPSCREENON"])
                {
                    var displayRequest = new Windows.System.Display.DisplayRequest();
                    displayRequest.RequestActive();
                }
            if (localSettings.Values.ContainsKey("RUDDERRESILIENCE"))
                rudderPad.Resilience = (bool)localSettings.Values["RUDDERRESILIENCE"];

            if (localSettings.Values.ContainsKey("BTNLABEL1"))
                flyButtonsPanel.Btn1Text = localSettings.Values["BTNLABEL1"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL2"))
                flyButtonsPanel.Btn2Text = localSettings.Values["BTNLABEL2"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL3"))
                flyButtonsPanel.Btn3Text = localSettings.Values["BTNLABEL3"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL4"))
                flyButtonsPanel.Btn4Text = localSettings.Values["BTNLABEL4"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL5"))
                flyButtonsPanel.Btn5Text = localSettings.Values["BTNLABEL5"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL6"))
                flyButtonsPanel.Btn6Text = localSettings.Values["BTNLABEL6"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL7"))
                flyButtonsPanel.Btn7Text = localSettings.Values["BTNLABEL7"].ToString();
            rudderPad.Value = App.rudderValue;
            throttleSlider.Value = App.throttleValue;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            App.rudderValue = rudderPad.Value;
            App.throttleValue = throttleSlider.Value;
        }

        private void throttleSlider_ValueChanged(object sender, SliderValueChangedEventArgs e)
        {
            App.actionHelper.OnSliderValueChanged(e.NewValue);
        }
        private void rudderPad_ValueChanged(object sender, SliderValueChangedEventArgs e)
        {
            App.actionHelper.OnRudderValueChanged(e.NewValue);
        }
        private void flyButtons_Pressed(object sender, ButtonsPanelEventArgs e)
        {
            App.actionHelper.OnButtonPressed(e.Button);
        }
        private void flyButtons_Released(object sender, ButtonsPanelEventArgs e)
        {
            App.actionHelper.OnButtonReleased(e.Button);
        }

        private async void conButton_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            try
            {
                //connecting
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                if (localSettings.Values.ContainsKey("IPADDR"))
                {
                    int trackPort;
                    if (localSettings.Values.ContainsKey("TRACKPORT"))
                    {
                        try
                        {
                            trackPort = int.Parse(localSettings.Values["TRACKPORT"].ToString());
                        }
                        catch (Exception)
                        {
                            trackPort = 4242;
                        }
                    }
                    else
                    {
                        this.Frame.Navigate(typeof(SettingsPage));
                        return;
                    }
                    await App.actionHelper.connectTo(localSettings.Values["IPADDR"].ToString(), trackPort);
                    conButton.Opacity = App.actionHelper.connected ? 1 : 0.7;
                    MsgBox.Text = App.actionHelper.connected ? "Online" : "Offline";
                }
                else
                {
                    this.Frame.Navigate(typeof(SettingsPage));
                }

            }
            catch (Exception exception)
            {
                switch (SocketError.GetStatus(exception.HResult))
                {
                    case SocketErrorStatus.HostNotFound:
                        // Handle HostNotFound Error
                        MsgBox.Text = "NotFound";
                        break;
                    default:
                        // Handle Unknown Error
                        MsgBox.Text = "FAILED";
                        break;
                }
            }
        }

        private void FlyButtonsPanel_TrackerBtnPressed(object sender, EventArgs e)
        {
            rightTrackBtnPressed = true;
            if (leftTrackBtnPressed)
                App.actionHelper.mode = ActionHelper.SensorMode.TRACKER;
        }

        private void FlyButtonsPanel_TrackerBtnReleased(object sender, EventArgs e)
        {
            rightTrackBtnPressed = false;
            if(!leftTrackBtnPressed)
            App.actionHelper.mode = ActionHelper.SensorMode.JOYSTICK;
        }

        private void RudderPad_TrackerBtnPressed(object sender, EventArgs e)
        {
            leftTrackBtnPressed = true;
            if (rightTrackBtnPressed)
                App.actionHelper.mode = ActionHelper.SensorMode.TRACKER;
        }

        private void RudderPad_TrackerBtnReleased(object sender, EventArgs e)
        {
            leftTrackBtnPressed = false;
            if(!rightTrackBtnPressed)
                App.actionHelper.mode = ActionHelper.SensorMode.JOYSTICK;
        }
        private void ComHelper_ConnectLose()
        {
            MsgBox.Text = "Lose";
            conButton.Opacity = 0.7;
        }
        private void ActionHelper_InclinometerStateChanged(object sender, EventArgs e)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (App.actionHelper.InclinometerState)
                {
                    case MagnetometerAccuracy.High:
                        MagnetStateBox.Foreground = new SolidColorBrush(Colors.Green);
                        MagnetStateBox.Text = "High"; break;
                    case MagnetometerAccuracy.Approximate:
                        MagnetStateBox.Foreground = new SolidColorBrush(Colors.YellowGreen);
                        MagnetStateBox.Text = "Approximate"; break;
                    case MagnetometerAccuracy.Unreliable:
                        MagnetStateBox.Foreground = new SolidColorBrush(Colors.OrangeRed);
                        MagnetStateBox.Text = "Unreliable"; break;
                    case MagnetometerAccuracy.Unknown:
                        MagnetStateBox.Foreground = new SolidColorBrush(Colors.Gray);
                        MagnetStateBox.Text = "Unknown"; break;
                }
            });
        }

        private void optButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }
    }
}
