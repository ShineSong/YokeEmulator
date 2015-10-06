using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using YokeCtl;
// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace YokeEmulator
{
    /// <summary>
    /// 7键位普通飞行页
    /// </summary>
    public sealed partial class ButtonPage : Page
    {
        Windows.System.Display.DisplayRequest displayRequest;
        GestureRecognizer logoGestureRecognizer = new GestureRecognizer();
        GestureRecognizer swipeAreaGestureRecognizer = new GestureRecognizer();

        bool leftTrackBtnPressed;
        bool rightTrackBtnPressed;
        Point LeftTouchPoint;
        Point rightTouchPoint;
        ActionHelper.SensorMode previousSensorMode;

        bool leftCaliBtnPressed;
        bool rightCaliBtnPressed;
        bool activeRequested;

        bool[] dtapped = null;
        public ButtonPage()
        {
            this.InitializeComponent();
            displayRequest = new Windows.System.Display.DisplayRequest();
            leftTrackBtnPressed = false;
            rightTrackBtnPressed = false;
            leftCaliBtnPressed = false;
            rightCaliBtnPressed = false;
            previousSensorMode = ActionHelper.SensorMode.JOYSTICK;
            dtapped = new bool[16];
            App.actionHelper.InclinometerStateChanged += ActionHelper_InclinometerStateChanged;

            logoGestureInputProcessor(logoGestureRecognizer);
            swipeAreaGestureInputProcessor(swipeAreaGestureRecognizer);
        }
        
        #region page function
        /// <summary>
        /// Read settings and set property for controls, and recover rudder, slider values
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("KEEPSCREENON"))
                if ((bool)localSettings.Values["KEEPSCREENON"])
                {
                    activeRequested = true;
                    displayRequest.RequestActive();
                }
                else
                    activeRequested = false;
            if (localSettings.Values.ContainsKey("BTNLABEL10"))
                Btn1Text.Text = localSettings.Values["BTNLABEL10"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL11"))
                Btn2Text.Text = localSettings.Values["BTNLABEL11"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL12"))
                Btn3Text.Text = localSettings.Values["BTNLABEL12"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL13"))
                Btn4Text.Text = localSettings.Values["BTNLABEL13"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL14"))
                Btn5Text.Text = localSettings.Values["BTNLABEL14"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL15"))
                Btn6Text.Text = localSettings.Values["BTNLABEL15"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL16"))
                Btn7Text.Text = localSettings.Values["BTNLABEL16"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL17"))
                Btn8Text.Text = localSettings.Values["BTNLABEL17"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL18"))
                Btn9Text.Text = localSettings.Values["BTNLABEL18"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL19"))
                Btn10Text.Text = localSettings.Values["BTNLABEL19"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL20"))
                Btn11Text.Text = localSettings.Values["BTNLABEL20"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL21"))
                Btn12Text.Text = localSettings.Values["BTNLABEL21"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL22"))
                Btn13Text.Text = localSettings.Values["BTNLABEL22"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL23"))
                Btn14Text.Text = localSettings.Values["BTNLABEL23"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL24"))
                Btn15Text.Text = localSettings.Values["BTNLABEL24"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL25"))
                Btn16Text.Text = localSettings.Values["BTNLABEL25"].ToString();

            Btn1.Opacity = dtapped[0] ? 1 : 0.5;
            Btn2.Opacity = dtapped[1] ? 1 : 0.5;
            Btn3.Opacity = dtapped[2] ? 1 : 0.5;
            Btn4.Opacity = dtapped[3] ? 1 : 0.5;
            Btn5.Opacity = dtapped[4] ? 1 : 0.5;
            Btn6.Opacity = dtapped[5] ? 1 : 0.5;
            Btn7.Opacity = dtapped[6] ? 1 : 0.5;
            Btn8.Opacity = dtapped[7] ? 1 : 0.5;
            Btn9.Opacity = dtapped[8] ? 1 : 0.5;
            Btn10.Opacity = dtapped[9] ? 1 : 0.5;
            Btn11.Opacity = dtapped[10] ? 1 : 0.5;
            Btn12.Opacity = dtapped[11] ? 1 : 0.5;
            Btn13.Opacity = dtapped[12] ? 1 : 0.5;
            Btn14.Opacity = dtapped[13] ? 1 : 0.5;
            Btn15.Opacity = dtapped[14] ? 1 : 0.5;
            Btn16.Opacity = dtapped[15] ? 1 : 0.5;

            throttleSlider.Value = App.throttleValue;
            leftSlider.Value = App.leftSliderValue;
            rightSlider.Value = App.rightSliderValue;
            dtapped = App.btnDTapped;

            switch (App.actionHelper.mode)
            {
                case ActionHelper.SensorMode.NONE:
                    logoImage.Opacity = 0.5;
                    logoImage.Source = joyLogoImage.Source;
                    break;
                case ActionHelper.SensorMode.TRACKER:
                    logoImage.Opacity = 1;
                    logoImage.Source = trackerLogoImage.Source;
                    break;
                case ActionHelper.SensorMode.JOYSTICK:
                    logoImage.Opacity = 1;
                    logoImage.Source = joyLogoImage.Source;
                    break;
                case ActionHelper.SensorMode.CALIBRATION:
                    logoImage.Opacity = 1;
                    logoImage.Source = caliLogoImage.Source;
                    break;
            }
            conButton.Source = App.actionHelper.connected ? linkImage.Source : unlinkImage.Source;
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
        }
        /// <summary>
        /// restore control value for temporary.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if(activeRequested)
                    displayRequest.RequestRelease();
            App.throttleValue = throttleSlider.Value;
            App.leftSliderValue = leftSlider.Value;
            App.rightSliderValue = rightSlider.Value;
            App.btnDTapped = dtapped;
        }
        #endregion

        #region controls event handler
        /// <summary>
        /// throttle value changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void throttleSlider_ValueChanged(object sender, SliderValueChangedEventArgs e)
        {
            App.actionHelper.OnSliderValueChanged(e.NewValue);
        }
        /// <summary>
        /// rudder value changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rudderPad_ValueChanged(object sender, SliderValueChangedEventArgs e)
        {
            App.actionHelper.OnRudderValueChanged(e.NewValue);
        }
        /// <summary>
        /// left slider value changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void leftSlider_ValueChanged(object sender, SliderValueChangedEventArgs e)
        {
            App.actionHelper.OnLeftSliderValueChanged(e.NewValue);
        }
        /// <summary>
        /// left slider value changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rightSlider_ValueChanged(object sender, SliderValueChangedEventArgs e)
        {
            App.actionHelper.OnRightValueChanged(e.NewValue);
        }
        /// <summary>
        /// buttons pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void flyButtons_Pressed(object sender, ButtonsPanelEventArgs e)
        {
            App.actionHelper.OnButtonPressed(e.Button);
        }
        /// <summary>
        /// buttons released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void flyButtons_Released(object sender, ButtonsPanelEventArgs e)
        {
            App.actionHelper.OnButtonReleased(e.Button);
        }

        private void ButtonPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Ellipse btn = (Ellipse)sender;
            btn.CapturePointer(e.Pointer);
            int idx = int.Parse((string)btn.Tag)-1;
            App.actionHelper.OnButtonPressed(10 + idx);
            btn.Opacity = 1;
            dtapped[idx] = false;
        }

        private void ButtonPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Ellipse btn = (Ellipse)sender;
            btn.ReleasePointerCapture(e.Pointer);
            int idx = int.Parse((string)btn.Tag) - 1;
            if (dtapped[idx])
            {
                App.actionHelper.OnButtonPressed(10 + idx);
                btn.Opacity = 1;
            }
            else
            {
                App.actionHelper.OnButtonReleased(10 + idx);
                btn.Opacity = 0.5;
            }
        }

        private void ButtonDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Ellipse btn = (Ellipse)sender;
            int bid = int.Parse((string)btn.Tag) - 1;
            dtapped[bid] = true;
        }
        /// <summary>
        /// right tracker button pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rightTrackerBtnPressed(object sender, PointerRoutedEventArgs e)
        {
            rightTrackerBtn.Opacity = 1;
            rightTrackerBtn.CapturePointer(e.Pointer);
            rightTrackBtnPressed = true;
            if (leftTrackBtnPressed)
            {
                previousSensorMode = App.actionHelper.mode;
                App.actionHelper.mode = ActionHelper.SensorMode.TRACKER;
                switch (App.actionHelper.mode)
                {
                    case ActionHelper.SensorMode.NONE:
                        logoImage.Opacity = 0.5;
                        logoImage.Source = joyLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.TRACKER:
                        logoImage.Opacity = 1;
                        logoImage.Source = trackerLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.JOYSTICK:
                        logoImage.Opacity = 1;
                        logoImage.Source = joyLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.CALIBRATION:
                        logoImage.Opacity = 1;
                        logoImage.Source = caliLogoImage.Source;
                        break;
                }
            }
            
        }
        /// <summary>
        /// right tracker button released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rightTrackerBtnReleased(object sender, PointerRoutedEventArgs e)
        {
            rightTrackerBtn.Opacity = 0.7;
            rightTrackerBtn.ReleasePointerCapture(e.Pointer);
            rightTrackBtnPressed = false;
            if(!leftTrackBtnPressed)
            {
                App.actionHelper.mode = previousSensorMode;
                switch (App.actionHelper.mode)
                {
                    case ActionHelper.SensorMode.NONE:
                        logoImage.Opacity = 0.5;
                        logoImage.Source = joyLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.TRACKER:
                        logoImage.Opacity = 1;
                        logoImage.Source = trackerLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.JOYSTICK:
                        logoImage.Opacity = 1;
                        logoImage.Source = joyLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.CALIBRATION:
                        logoImage.Opacity = 1;
                        logoImage.Source = caliLogoImage.Source;
                        break;
                }
            }

        }
        /// <summary>
        /// right tracker button moved(zoom)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rightTrackerBtn_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            rightTouchPoint = e.GetCurrentPoint(null).Position;
            if (leftTrackBtnPressed)
            {
                double dx = rightTouchPoint.X - LeftTouchPoint.X;
                double dy = rightTouchPoint.Y - LeftTouchPoint.Y;
                App.comHelper._trackZ = -Math.Atan2(dy, dx) * 100 / Math.PI;
            }
        }
        /// <summary>
        /// left tracker button pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void leftTrackerBtnPressed(object sender, PointerRoutedEventArgs e)
        {
            leftTrackBtnPressed = true;
            if (rightTrackBtnPressed)
            {
                previousSensorMode = App.actionHelper.mode;
                App.actionHelper.mode = ActionHelper.SensorMode.TRACKER;
                switch (App.actionHelper.mode)
                {
                    case ActionHelper.SensorMode.NONE:
                        logoImage.Opacity = 0.5;
                        logoImage.Source = joyLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.TRACKER:
                        logoImage.Opacity = 1;
                        logoImage.Source = trackerLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.JOYSTICK:
                        logoImage.Opacity = 1;
                        logoImage.Source = joyLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.CALIBRATION:
                        logoImage.Opacity = 1;
                        logoImage.Source = caliLogoImage.Source;
                        break;
                }
            }
        }
        /// <summary>
        /// left tracker button released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void leftTrackerBtnReleased(object sender, PointerRoutedEventArgs e)
        {
            leftTrackBtnPressed = false;
            if(!rightTrackBtnPressed)
            {
                App.actionHelper.mode = previousSensorMode;
                switch (App.actionHelper.mode)
                {
                    case ActionHelper.SensorMode.NONE:
                        logoImage.Opacity = 0.5;
                        logoImage.Source = joyLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.TRACKER:
                        logoImage.Opacity = 1;
                        logoImage.Source = trackerLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.JOYSTICK:
                        logoImage.Opacity = 1;
                        logoImage.Source = joyLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.CALIBRATION:
                        logoImage.Opacity = 1;
                        logoImage.Source = caliLogoImage.Source;
                        break;
                }
            }
        }
        /// <summary>
        /// left tracker button moved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void leftTrackerBtn_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            LeftTouchPoint = e.GetCurrentPoint(null).Position;
            if (rightTrackBtnPressed)
            {
                double dx = rightTouchPoint.X - LeftTouchPoint.X;
                double dy = rightTouchPoint.Y - LeftTouchPoint.Y;
                App.comHelper._trackZ = -Math.Atan2(dy, dx) * 100 / Math.PI;
            }
        }
        /// <summary>
        /// left calibration btn pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void leftCalibrationBtn_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            leftCaliBtnPressed = true;
            if (rightCaliBtnPressed)
            {
                previousSensorMode = App.actionHelper.mode;
                App.actionHelper.mode = ActionHelper.SensorMode.CALIBRATION;
                MsgBox.Text = "Calibrating";
                switch (App.actionHelper.mode)
                {
                    case ActionHelper.SensorMode.NONE:
                        logoImage.Opacity = 0.5;
                        logoImage.Source = joyLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.TRACKER:
                        logoImage.Opacity = 1;
                        logoImage.Source = trackerLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.JOYSTICK:
                        logoImage.Opacity = 1;
                        logoImage.Source = joyLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.CALIBRATION:
                        logoImage.Opacity = 1;
                        logoImage.Source = caliLogoImage.Source;
                        break;
                }
            }
        }
        /// <summary>
        /// left calibration btn released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void leftCalibrationBtn_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            leftCaliBtnPressed = false;
            App.actionHelper.mode = previousSensorMode;
            MsgBox.Text = "";
            switch (App.actionHelper.mode)
            {
                case ActionHelper.SensorMode.NONE:
                    logoImage.Opacity = 0.5;
                    logoImage.Source = joyLogoImage.Source;
                    break;
                case ActionHelper.SensorMode.TRACKER:
                    logoImage.Opacity = 1;
                    logoImage.Source = trackerLogoImage.Source;
                    break;
                case ActionHelper.SensorMode.JOYSTICK:
                    logoImage.Opacity = 1;
                    logoImage.Source = joyLogoImage.Source;
                    break;
                case ActionHelper.SensorMode.CALIBRATION:
                    logoImage.Opacity = 1;
                    logoImage.Source = caliLogoImage.Source;
                    break;
            }
        }
        /// <summary>
        /// right calibration btn pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rightCalibrationBtn_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            rightCaliBtnPressed = true;
            if (leftCaliBtnPressed)
            {
                previousSensorMode = App.actionHelper.mode;
                App.actionHelper.mode = ActionHelper.SensorMode.CALIBRATION;
                MsgBox.Text = "Calibrating";
                switch (App.actionHelper.mode)
                {
                    case ActionHelper.SensorMode.NONE:
                        logoImage.Opacity = 0.5;
                        logoImage.Source = joyLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.TRACKER:
                        logoImage.Opacity = 1;
                        logoImage.Source = trackerLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.JOYSTICK:
                        logoImage.Opacity = 1;
                        logoImage.Source = joyLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.CALIBRATION:
                        logoImage.Opacity = 1;
                        logoImage.Source = caliLogoImage.Source;
                        break;
                }
            }
        }
        /// <summary>
        /// right calibration btn released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rightCalibrationBtn_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            rightCaliBtnPressed = false;
            App.actionHelper.mode = previousSensorMode;
            MsgBox.Text = "";
            switch (App.actionHelper.mode)
            {
                case ActionHelper.SensorMode.NONE:
                    logoImage.Opacity = 0.5;
                    logoImage.Source = joyLogoImage.Source;
                    break;
                case ActionHelper.SensorMode.TRACKER:
                    logoImage.Opacity = 1;
                    logoImage.Source = trackerLogoImage.Source;
                    break;
                case ActionHelper.SensorMode.JOYSTICK:
                    logoImage.Opacity = 1;
                    logoImage.Source = joyLogoImage.Source;
                    break;
                case ActionHelper.SensorMode.CALIBRATION:
                    logoImage.Opacity = 1;
                    logoImage.Source = caliLogoImage.Source;
                    break;
            }
        }
        /// <summary>
        /// show inclinometer yaw accuracy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        #endregion

        #region system buttons
        /// <summary>
        /// connect to server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void conButton_Tapped(object sender, TappedRoutedEventArgs e)
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
                try
                {
                    await App.actionHelper.connectTo(localSettings.Values["IPADDR"].ToString(), 23333, 23334, trackPort);
                }
                catch (Exception)
                {
                    this.Frame.Navigate(typeof(SettingsPage));
                    return;
                }
                conButton.Source = App.actionHelper.connected ? linkImage.Source : unlinkImage.Source;
            }
            else
            {
                this.Frame.Navigate(typeof(SettingsPage));
            }
        }
        /// <summary>
        /// navigate to setting page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            
            this.Frame.Navigate(typeof(SettingsPage));
        }
        #endregion

        #region Gesture
        /// <summary>
        /// logo gesture register (swipe up and down)
        /// </summary>
        /// <param name="gr"></param>
        private void logoGestureInputProcessor(Windows.UI.Input.GestureRecognizer gr)
        {
            this.logoGestureRecognizer = gr;
            this.logoGestureRecognizer.GestureSettings = GestureSettings.ManipulationTranslateY | GestureSettings.ManipulationTranslateX;
            
            this.logoImage.PointerCanceled += OnLogoPointerCanceled;
            this.logoImage.PointerPressed += OnLogoPointerPressed;
            this.logoImage.PointerReleased += OnLogoPointerReleased;
            this.logoImage.PointerMoved += OnLogoPointerMoved;
            
            logoGestureRecognizer.ManipulationCompleted += LogoGestureRecognizer_ManipulationCompleted;
        }
        /// <summary>
        /// gesture completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void LogoGestureRecognizer_ManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            if (args.Velocities.Linear.Y > 0.5 && args.Cumulative.Translation.Y > 20)
                MsgBox.Text = "SwipeDown";
            else if (args.Velocities.Linear.Y < -0.5 && args.Cumulative.Translation.Y < -20)
                MsgBox.Text = "SwipeUp";
            if (args.Velocities.Linear.X > 0.5 && args.Cumulative.Translation.X > 50)
            {
                int mode = (int)App.actionHelper.mode + 1;
                mode = mode % 3;
                App.actionHelper.mode = (ActionHelper.SensorMode)mode;
                switch (App.actionHelper.mode)
                {
                    case ActionHelper.SensorMode.NONE:
                        logoImage.Opacity = 0.5;
                        logoImage.Source = joyLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.TRACKER:
                        logoImage.Opacity = 1;
                        logoImage.Source = trackerLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.JOYSTICK:
                        logoImage.Opacity = 1;
                        logoImage.Source = joyLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.CALIBRATION:
                        logoImage.Opacity = 1;
                        logoImage.Source = caliLogoImage.Source;
                        break;
                }
            }
            else if (args.Velocities.Linear.X < -0.5 && args.Cumulative.Translation.X < -50)
            {
                int mode = (int)App.actionHelper.mode - 1;
                if (mode < 0) mode = 2;
                mode = mode % 3;
                App.actionHelper.mode = (ActionHelper.SensorMode)mode;
                switch (App.actionHelper.mode)
                {
                    case ActionHelper.SensorMode.NONE:
                        logoImage.Opacity = 0.5;
                        logoImage.Source = joyLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.TRACKER:
                        logoImage.Opacity = 1;
                        logoImage.Source = trackerLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.JOYSTICK:
                        logoImage.Opacity = 1;
                        logoImage.Source = joyLogoImage.Source;
                        break;
                    case ActionHelper.SensorMode.CALIBRATION:
                        logoImage.Opacity = 1;
                        logoImage.Source = caliLogoImage.Source;
                        break;
                }
            }
        }

        
        void OnLogoPointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            UIElement element = (UIElement)sender;
            this.logoGestureRecognizer.ProcessDownEvent(args.GetCurrentPoint(element));
            element.CapturePointer(args.Pointer);
            args.Handled = true;
        }

        void OnLogoPointerCanceled(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            this.logoGestureRecognizer.CompleteGesture();
            args.Handled = true;
        }

        void OnLogoPointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            this.logoGestureRecognizer.ProcessUpEvent(args.GetCurrentPoint(this.logoImage));
            args.Handled = true;
        }

        void OnLogoPointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            this.logoGestureRecognizer.ProcessMoveEvents(args.GetIntermediatePoints(this.logoImage));
        }
        /// <summary>
        /// swipe area gesture register ( swipe left and right )
        /// </summary>
        /// <param name="gr"></param>
        private void swipeAreaGestureInputProcessor(Windows.UI.Input.GestureRecognizer gr)
        {
            this.swipeAreaGestureRecognizer = gr;
            this.swipeAreaGestureRecognizer.GestureSettings = GestureSettings.ManipulationTranslateX;
            
            this.SwipeArea.PointerCanceled += OnSwipeAreaPointerCanceled;
            this.SwipeArea.PointerPressed += OnSwipeAreaPointerPressed;
            this.SwipeArea.PointerReleased += OnSwipeAreaPointerReleased;
            this.SwipeArea.PointerMoved += OnSwipeAreaPointerMoved;

            swipeAreaGestureRecognizer.ManipulationCompleted += swipeAreaGestureRecognizer_ManipulationCompleted;
        }
        /// <summary>
        /// gesture completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void swipeAreaGestureRecognizer_ManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            if (args.Velocities.Linear.X > 1 && args.Cumulative.Translation.X > 50)
                this.Frame.Navigate(typeof(BattlePage));
            else if (args.Velocities.Linear.X < -1 && args.Cumulative.Translation.X < -50)
                this.Frame.Navigate(typeof(FlyPage));
        }

        void OnSwipeAreaPointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            UIElement element = (UIElement)sender;
            this.swipeAreaGestureRecognizer.ProcessDownEvent(args.GetCurrentPoint(element));
            element.CapturePointer(args.Pointer);
            args.Handled = true;
        }

        void OnSwipeAreaPointerCanceled(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            this.swipeAreaGestureRecognizer.CompleteGesture();
            args.Handled = true;
        }

        void OnSwipeAreaPointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            this.swipeAreaGestureRecognizer.ProcessUpEvent(args.GetCurrentPoint(this.SwipeArea));
            args.Handled = true;
        }

        void OnSwipeAreaPointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            this.swipeAreaGestureRecognizer.ProcessMoveEvents(args.GetIntermediatePoints(this.SwipeArea));
        }
        #endregion

    }
}
