using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class ButtonsPage : Page
    {
        byte[] isToggle = null;
        public ButtonsPage()
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
            int btnCount = 0;
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
//                        _item.PointerEntered += buttonsPressed;
//                       _item.PointerExited += buttonsReleased;
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
        }

        private void appBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
