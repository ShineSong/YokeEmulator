using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Foundation.Metadata;

namespace YokeCtl
{
    public sealed class SliderValueChangedEventArgs : RoutedEventArgs
    {
        public SliderValueChangedEventArgs(double _new,double _old,object _s)
        {
            _NewValue = _new;
            _OldValue = _old;
            _Source = _s;
        }
        double _NewValue;
        double _OldValue;
        object _Source;
        public double NewValue
        {
            get { return _NewValue; }
            set { _NewValue = (double)value; }
        }
        public double OldValue
        {
            get { return _OldValue; }
            set { _OldValue = (double)value; }
        }
        public object OriginalSource
        {
            get { return _Source; }
            set { _Source = (object)value; }
        }
    }
    public delegate void SliderValueChangedEventHandler(object sender, SliderValueChangedEventArgs e);
}
