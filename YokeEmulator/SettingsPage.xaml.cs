using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace YokeEmulator
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey("IPADDR"))
                ipTextBox.Text = localSettings.Values["IPADDR"].ToString();
            else
                ipTextBox.Text = "192.168.1.101";


            if (localSettings.Values.ContainsKey("TRACKPORT"))
                trackPortTextBox.Text = localSettings.Values["TRACKPORT"].ToString();
            else
                trackPortTextBox.Text = "4242";
            if (localSettings.Values.ContainsKey("KEEPSCREENON"))
                keepScreen.IsOn = (bool)localSettings.Values["KEEPSCREENON"];
            else
                keepScreen.IsOn = false;

            if (localSettings.Values.ContainsKey("RUDDERRESILIENCE"))
                rudderResilience.IsOn = (bool)localSettings.Values["RUDDERRESILIENCE"];
            else
                rudderResilience.IsOn = false;

            if (localSettings.Values.ContainsKey("BTNLABEL1"))
                flybtn1TextBox.Text = localSettings.Values["BTNLABEL1"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL2"))
                flybtn2TextBox.Text = localSettings.Values["BTNLABEL2"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL3"))
                flybtn3TextBox.Text = localSettings.Values["BTNLABEL3"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL4"))
                flybtn4TextBox.Text = localSettings.Values["BTNLABEL4"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL5"))
                flybtn5TextBox.Text = localSettings.Values["BTNLABEL5"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL6"))
                flybtn6TextBox.Text = localSettings.Values["BTNLABEL6"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL7"))
                flybtn7TextBox.Text = localSettings.Values["BTNLABEL7"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL8"))
                battlebtn1TextBox.Text = localSettings.Values["BTNLABEL8"].ToString();
            if (localSettings.Values.ContainsKey("BTNLABEL9"))
                battlebtn2TextBox.Text = localSettings.Values["BTNLABEL9"].ToString();
            
        }

        private void acceptBtn_Click(object sender, RoutedEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["IPADDR"] = ipTextBox.Text;
            localSettings.Values["TRACKPORT"] = trackPortTextBox.Text;
            localSettings.Values["KEEPSCREENON"] = keepScreen.IsOn;
            localSettings.Values["RUDDERRESILIENCE"] = rudderResilience.IsOn;
            
            localSettings.Values["BTNLABEL1"] = flybtn1TextBox.Text;
            localSettings.Values["BTNLABEL2"] = flybtn2TextBox.Text;
            localSettings.Values["BTNLABEL3"] = flybtn3TextBox.Text;
            localSettings.Values["BTNLABEL4"] = flybtn4TextBox.Text;
            localSettings.Values["BTNLABEL5"] = flybtn5TextBox.Text;
            localSettings.Values["BTNLABEL6"] = flybtn6TextBox.Text;
            localSettings.Values["BTNLABEL7"] = flybtn7TextBox.Text;
            localSettings.Values["BTNLABEL8"] = battlebtn1TextBox.Text;
            localSettings.Values["BTNLABEL9"] = battlebtn2TextBox.Text;

            this.Frame.GoBack();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}
