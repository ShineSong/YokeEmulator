using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.Foundation.Metadata;
namespace YokeCtl
{
    public sealed class ButtonsPanelEventArgs : RoutedEventArgs
    {
        public enum State { Pressed,Released};
        int _bid;
        State _state;
        public ButtonsPanelEventArgs(int bid,State s)
        {
            _bid = bid;
            _state = s;
        }
        
        public int Button
        {
            get { return _bid; }
        }
        public State ButtonState
        {
            get { return _state; }
        }
    }

    public delegate void ButtonsPanelEventHandler(object sender, ButtonsPanelEventArgs e);
}
