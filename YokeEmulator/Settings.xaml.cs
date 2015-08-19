using YokeEmulator.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “基本页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace YokeEmulator
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Settings : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        int btnCount;
        public Settings()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            
        }

        /// <summary>
        /// 获取与此 <see cref="Page"/> 关联的 <see cref="NavigationHelper"/>。
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// 获取此 <see cref="Page"/> 的视图模型。
        /// 可将其更改为强类型视图模型。
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// 使用在导航过程中传递的内容填充页。  在从以前的会话
        /// 重新创建页时，也会提供任何已保存状态。
        /// </summary>
        /// <param name="sender">
        /// 事件的来源；通常为 <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">事件数据，其中既提供在最初请求此页时传递给
        /// <see cref="Frame.Navigate(Type, Object)"/> 的导航参数，又提供
        /// 此页在以前会话期间保留的状态的
        /// 字典。首次访问页面时，该状态将为 null。</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// 保留与此页关联的状态，以防挂起应用程序或
        /// 从导航缓存中放弃此页。值必须符合
        /// <see cref="SuspensionManager.SessionState"/> 的序列化要求。
        /// </summary>
        /// <param name="sender">事件的来源；通常为 <see cref="NavigationHelper"/></param>
        ///<param name="e">提供要使用可序列化状态填充的空字典
        ///的事件数据。</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper 注册

        /// <summary>
        /// 此部分中提供的方法只是用于使
        /// NavigationHelper 可响应页面的导航方法。
        /// <para>
        /// 应将页面特有的逻辑放入用于
        /// <see cref="NavigationHelper.LoadState"/>
        /// 和 <see cref="NavigationHelper.SaveState"/> 的事件处理程序中。
        /// 除了在会话期间保留的页面状态之外
        /// LoadState 方法中还提供导航参数。
        /// </para>
        /// </summary>
        /// <param name="e">提供导航方法数据和
        /// 无法取消导航请求的事件处理程序。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey("IPADDR")) 
                ipTextBox.Text = localSettings.Values["IPADDR"].ToString();
            else
                ipTextBox.Text = "192.168.1.101";

            if (localSettings.Values.ContainsKey("BTNCOUNT"))
                btnCountTextBox.Text = localSettings.Values["BTNCOUNT"].ToString();
            else
                btnCountTextBox.Text = "0";

            if (localSettings.Values.ContainsKey("TRACKPORT"))
                trackPortTextBox.Text = localSettings.Values["TRACKPORT"].ToString();
            else
                trackPortTextBox.Text = "4242";

            try
            {
                btnCount = int.Parse(btnCountTextBox.Text);
            }
            catch (Exception)
            {
                btnCount = 0;
            }

            for (int i = 0; i < btnCount; ++i)
            {
                StackPanel _panel = new StackPanel();
                _panel.Orientation = Orientation.Horizontal;
                TextBox _item = new TextBox();
                _item.HorizontalAlignment = HorizontalAlignment.Left;
                ToggleSwitch _switch = new ToggleSwitch();
                _switch.OffContent = "按钮"; //松手改变状态
                _switch.OnContent = "开关"; //松手不改变状态
                _panel.Children.Add(_item);
                _panel.Children.Add(_switch);
                editStack.Children.Add(_panel);
            }
            fillBtnLabels();

            if (localSettings.Values.ContainsKey("KEEPSCREENON"))
                keepScreen.IsOn = (bool)localSettings.Values["KEEPSCREENON"];
            else
                keepScreen.IsOn = false;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void acceptBtn_Click(object sender, RoutedEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["IPADDR"] = ipTextBox.Text;

            try
            {
                btnCount = int.Parse(btnCountTextBox.Text);
            }
            catch (Exception)
            {
                btnCount = 0;
            }
            localSettings.Values["BTNCOUNT"] = btnCountTextBox.Text;
            localSettings.Values["TRACKPORT"] = trackPortTextBox.Text;

            int count = editStack.Children.Count;
            for (int i = 0; i < count; ++i)
            {
                string key1 = "BTNLABEL" + i;
                string key2 = "BTNTOGGLE" + i;
                StackPanel _panel = (StackPanel)editStack.Children[i];
                TextBox _box = (TextBox)_panel.Children[0];
                ToggleSwitch _switch= (ToggleSwitch)_panel.Children[1];
                localSettings.Values[key1] = _box.Text;
                localSettings.Values[key2] = _switch.IsOn;
            }

            localSettings.Values["KEEPSCREENON"] = keepScreen.IsOn;
            this.Frame.GoBack();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void btnCountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                btnCount = int.Parse(btnCountTextBox.Text);
            }catch(Exception)
            {
                return;
            }
            if (btnCount > 128 || btnCount < 0)
                return;
            int curcount = editStack.Children.Count;
            int diff = btnCount - curcount;
            if (diff > 0)
            {
                for (int i = 0; i < diff; ++i)
                {
                    StackPanel _panel = new StackPanel();
                    _panel.Orientation = Orientation.Horizontal;

                    TextBox _item = new TextBox();
                    _item.HorizontalAlignment = HorizontalAlignment.Left;
                    ToggleSwitch _switch = new ToggleSwitch();
                    _switch.OffContent = "按钮"; //松手改变状态
                    _switch.OnContent = "开关"; //松手不改变状态
                    _panel.Children.Add(_item);
                    _panel.Children.Add(_switch);
                    editStack.Children.Add(_panel);
                }
            }
            else if (diff < 0)
            {
                for (int i = 0; i < -diff; ++i)
                {
                    editStack.Children.RemoveAt(editStack.Children.Count - 1);
                }
            }
            else
            {
                return;
            }
            fillBtnLabels();
        }
        private void fillBtnLabels()
        {
            var localSettings= Windows.Storage.ApplicationData.Current.LocalSettings;
            int count = editStack.Children.Count;
            for (int i = 0; i < count; ++i)
            {
                string key1 = "BTNLABEL" + i;
                string key2 = "BTNTOGGLE" + i;
                StackPanel _panel = (StackPanel)editStack.Children[i];
                TextBox _box = (TextBox)_panel.Children[0];
                ToggleSwitch _switch = (ToggleSwitch)_panel.Children[1];
                if (localSettings.Values.ContainsKey(key1))
                {
                    _box.Text = localSettings.Values[key1].ToString();
                }
                else
                {
                    _box.Text = "BTN" + i;
                }
                if (localSettings.Values.ContainsKey(key2))
                {
                    _switch.IsOn = (bool)localSettings.Values[key2];
                }
                else
                {
                    _switch.IsOn = false;
                }
            }
        }
    }
}
