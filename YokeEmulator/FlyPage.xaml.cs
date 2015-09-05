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
using YokeCtl;
// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace YokeEmulator
{
    /// <summary>
    /// 7键位普通飞行页
    /// </summary>
    public sealed partial class FlyPage : Page
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
        public FlyPage()
        {
            this.InitializeComponent();
            displayRequest = new Windows.System.Display.DisplayRequest();
            leftTrackBtnPressed = false;
            rightTrackBtnPressed = false;
            leftCaliBtnPressed = false;
            rightCaliBtnPressed = false;
            previousSensorMode = ActionHelper.SensorMode.JOYSTICK;
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
            leftSlider.Value = App.leftSliderValue;
            rightSlider.Value = App.rightSliderValue;
            flyButtonsPanel.DoubleTappedProperty= App.flyBtnPanelDTapped;

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
            App.rudderValue = rudderPad.Value;
            App.throttleValue = throttleSlider.Value;
            App.leftSliderValue = leftSlider.Value;
            App.rightSliderValue = rightSlider.Value;
            App.flyBtnPanelDTapped = flyButtonsPanel.DoubleTappedProperty;
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
        /// <summary>
        /// right tracker button pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rightTrackerBtnPressed(object sender, EventArgs e)
        {
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
        private void rightTrackerBtnReleased(object sender, EventArgs e)
        {
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
        private void leftTrackerBtnPressed(object sender, EventArgs e)
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
        private void leftTrackerBtnReleased(object sender, EventArgs e)
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
                await App.actionHelper.connectTo(localSettings.Values["IPADDR"].ToString(), 23333, 23334, trackPort);
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
                this.Frame.Navigate(typeof(BattlePage));
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
