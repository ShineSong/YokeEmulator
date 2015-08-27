using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// “用户控件”项模板在 http://go.microsoft.com/fwlink/?LinkId=234235 上有介绍

namespace YokeCtl
{
    [TemplatePart(Name = CanvasName,Type =typeof(Canvas))]
    [TemplatePart(Name = line1Name, Type = typeof(Line))]
    [TemplatePart(Name = thumbName, Type = typeof(Ellipse))]
    [TemplatePart(Name = line2Name, Type = typeof(Line))]
    [TemplatePart(Name =textName,Type = typeof(TextBlock))]
    [TemplatePart(Name = textGridName,Type = typeof(Grid))]
    public sealed class YESlider : Control
    {
        private const string CanvasName = "PART_Canvas";
        private const string line1Name = "PART_Line1";
        private const string thumbName = "PART_Thumb";
        private const string line2Name = "PART_Line2";
        private const string textName = "PART_Reading";
        private const string textGridName = "PART_Readingbox";
        private Canvas _canvas = null;
        private Line _line1 = null;
        private Ellipse _thumb = null;
        private Line _line2 = null;
        private TextBlock _text = null;
        private Grid _textGrid = null;
        public YESlider()
        {
            this.DefaultStyleKey = typeof(YESlider);
        }
        #region ThumbSize
        public static readonly DependencyProperty ThumbSizeProperty =
            DependencyProperty.Register(
                "ThumbSize",
                typeof(double),
                typeof(YESlider),
                new PropertyMetadata(20,OnThumbSizeChanged));
        public double ThumbSize {
            get { return (int)GetValue(ThumbSizeProperty);}
            set { SetValue(ThumbSizeProperty, value); }
        }
        private static void OnThumbSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (YESlider)d;
            if (target._thumb != null)
            {
                target._thumb.Width = (double)e.NewValue;
                target._thumb.Height = (double)e.NewValue;
                target._textGrid.Width = (double)e.NewValue;
                target._textGrid.Height = (double)e.NewValue;
            }
        }
        #endregion
        #region Value
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(double),
                typeof(YESlider),
                new PropertyMetadata(0, OnValueChanged));
        public double Value
        {
            get { return (double)GetValue(ValueProperty);}
            set { SetValue(ValueProperty, value); }
        }
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (YESlider)d;
            if(target._canvas!=null)
                target.UpdateSlider();
        }
        public event SliderValueChangedEventHandler ValueChanged;
        #endregion

        #region StrokeThickness
        public static readonly DependencyProperty StrokeThicknessProperty = 
            DependencyProperty.Register(
                "StrokeThickness",
                typeof(double),
                typeof(YESlider),
                new PropertyMetadata(5));
        public double StrockThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        #endregion


        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _canvas = (Canvas)GetTemplateChild(CanvasName);
            _line1 = (Line)GetTemplateChild(line1Name);
            _line2 = (Line)GetTemplateChild(line2Name);
            _thumb = (Ellipse)GetTemplateChild(thumbName);
            _text = (TextBlock)GetTemplateChild(textName);
            _textGrid = (Grid)GetTemplateChild(textGridName);

            _line1.StrokeThickness = StrockThickness;
            _line2.StrokeThickness = StrockThickness;
            _thumb.StrokeThickness = StrockThickness;

            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSlider();
        }

        protected void UpdateSlider()
        {
            try
            {
                double _height = ActualHeight - _thumb.ActualHeight;

                _line1.X1 = ActualWidth * 0.5;
                _line1.X2 = ActualWidth * 0.5;
                _line2.X1 = ActualWidth * 0.5;
                _line2.X2 = ActualWidth * 0.5;
                
                _line1.Y1 = ActualHeight;
                _line1.Y2 = ActualHeight - Value * 0.01 * _height;
                _line2.Y1 = ActualHeight - Value * 0.01 * _height - _thumb.ActualHeight;
                _line2.Y2 = 0;
                _thumb.Margin = new Thickness((ActualWidth-_thumb.ActualWidth)*0.5, _line2.Y1, (ActualWidth+_thumb.ActualWidth)*0.5, _line1.Y2);
                _textGrid.Margin = new Thickness((ActualWidth - _thumb.ActualWidth) * 0.5, _line2.Y1+_thumb.ActualWidth/3, (ActualWidth + _thumb.ActualWidth) * 0.5, _line1.Y2);
                _text.Text = Value.ToString("0.00");
            }
            catch (Exception)
            {
                return;
            }
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(_canvas).Position;
            double old = Value;
            double _value = (ActualHeight - point.Y - _thumb.ActualHeight * 0.5) / (ActualHeight - _thumb.ActualWidth) * 100;
            if (_value > 100) Value = 100; else if (_value < 0) Value = 0; else Value = _value; ;
            if(ValueChanged!=null)
                ValueChanged(this,new SliderValueChangedEventArgs(old,Value, this));
            base.OnPointerEntered(e);
        }
        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(_canvas).Position;
            double old = Value;
            double _value = (ActualHeight - point.Y - _thumb.ActualHeight * 0.5) / (ActualHeight - _thumb.ActualWidth) * 100;
            if (_value > 100) Value = 100; else if (_value < 0) Value = 0; else Value = _value; ;
            if (ValueChanged != null)
                ValueChanged(this, new SliderValueChangedEventArgs(old, Value, this));
            base.OnPointerMoved(e);
        }
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(_canvas).Position;
            double old = Value;
            double _value = (ActualHeight - point.Y - _thumb.ActualHeight * 0.5) / (ActualHeight - _thumb.ActualWidth) * 100;
            if (_value > 100) Value = 100; else if (_value < 0) Value = 0; else Value = _value; ;
            if (ValueChanged != null)
                ValueChanged(this, new SliderValueChangedEventArgs(old, Value, this));
            base.OnPointerPressed(e);
            CapturePointer(e.Pointer);
        }
        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
        }
    }
}