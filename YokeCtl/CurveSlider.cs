using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// “用户控件”项模板在 http://go.microsoft.com/fwlink/?LinkId=234235 上有介绍

namespace YokeCtl
{
    [TemplatePart(Name = pathName, Type = typeof(Path))]
    [TemplatePart(Name = thumbGridName, Type = typeof(Grid))]
    [TemplatePart(Name = readingName, Type = typeof(TextBlock))]
    public sealed class CurveSlider : Control
    {
        private const string pathName = "PART_Path";
        private const string readingName = "PART_Reading";
        private const string thumbGridName = "PART_ThumbGrid";

        private Path _path;
        private Grid _thumbGrid;
        private TextBlock _readingBlock;

        public enum Orientations { LeftToRight,RightToLeft }
        public CurveSlider()
        {
            this.DefaultStyleKey = typeof(CurveSlider);
        }
        #region ThumbSize
        public static readonly DependencyProperty ThumbSizeProperty =
            DependencyProperty.Register(
                "ThumbSize",
                typeof(double),
                typeof(CurveSlider),
                new PropertyMetadata(20d));
        public double ThumbSize
        {
            get { return (double)GetValue(ThumbSizeProperty); }
            set { SetValue(ThumbSizeProperty, value); }
        }
        #endregion
        #region Value
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(double),
                typeof(CurveSlider),
                new PropertyMetadata(0d, OnValueChanged));
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (CurveSlider)d;
            if (target._thumbGrid != null)
                target.UpdateSlider();
        }

        public event SliderValueChangedEventHandler ValueChanged;
        #endregion
        #region Stroke
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(
                "Stroke",
                typeof(Brush),
                typeof(CurveSlider),
                new PropertyMetadata(new SolidColorBrush(Colors.White)));
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }
        #endregion
        #region StrokeThickness
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                "StrokeThickness",
                typeof(double),
                typeof(CurveSlider),
                new PropertyMetadata(5d));
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        #endregion
        #region Orientation
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                "Orientation",
                typeof(Orientations),
                typeof(CurveSlider),
                new PropertyMetadata(Orientations.LeftToRight));
        private PathFigure _bfigure;
        private BezierSegment _bseg;

        public Orientations Orientation
        {
            get { return (Orientations)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        #endregion

        Point cubicBezier(double t)
        {
            double x = Math.Pow(1 - t, 3) * _bfigure.StartPoint.X + 3 * Math.Pow(1 - t, 2) * t * _bseg.Point1.X + 3 * (1 - t) * Math.Pow(t, 2) * _bseg.Point2.X + Math.Pow(t, 3) * _bseg.Point3.X;
            double y = Math.Pow(1 - t, 3) * _bfigure.StartPoint.Y + 3 * Math.Pow(1 - t, 2) * t * _bseg.Point1.Y + 3 * (1 - t) * Math.Pow(t, 2) * _bseg.Point2.Y + Math.Pow(t, 3) * _bseg.Point3.Y;
            return new Point(x, y);
        }
        private double bezierX2t(double x)
        {
            //shengjin formula to solve cubic equation
            double a = -_bfigure.StartPoint.X + 3 * _bseg.Point1.X - 3 * _bseg.Point2.X + _bseg.Point3.X;
            double b = 3 * _bfigure.StartPoint.X - 6 * _bseg.Point1.X + 3 * _bseg.Point2.X;
            double c = -3 * _bfigure.StartPoint.X + 3 * _bseg.Point1.X;
            double d = _bfigure.StartPoint.X - x;

            double A = b * b - 3 * a * c;
            double B = b * c - 9 * a * d;
            double C = c * c - 3 * b * d;
            if (A == 0 && B == 0)
                return -b / 3 / a;
            double delta = B * B - 4 * A * C;
            if (delta > 0)
            {
                double Y1 = A * b + 3 * a * (-B + Math.Sqrt(B * B - 4 * A * C)) * 0.5;
                double Y2 = A * b + 3 * a * (-B - Math.Sqrt(B * B - 4 * A * C)) * 0.5;
                double X = (-b - Math.Pow(Y1, 1.0 / 3.0) - Math.Pow(Y2, 1.0 / 3.0));
                if (X >= 0 && X <= 1)
                    return X;
                else
                    throw new ArgumentException();
            }
            else if (delta < 0)
            {
                double theta = Math.Acos((2 * A * b - 3 * a * B) / 2 / Math.Pow(A, 1.5));
                double X1 = (-b - 2 * Math.Sqrt(A) * Math.Cos(theta / 3)) / 3 / a;
                double X2 = (-b + Math.Sqrt(A) * (Math.Cos(theta / 3) + Math.Sqrt(3) * Math.Sin(theta / 3))) / 3 / a;
                double X3 = (-b + Math.Sqrt(A) * (Math.Cos(theta / 3) - Math.Sqrt(3) * Math.Sin(theta / 3))) / 3 / a;
                if (X1 >= 0 && X1 <= 1)
                    return X1;
                else if (X2 >= 0 && X2 <= 1)
                    return X2;
                else if (X3 >= -0.0001 && X3 <= 1.0001)
                    return X3;
                else
                    throw new ArgumentException();
            }
            else
            {
                double X1 = -b / a + B / A;
                double X2 = -B / A / 2;
                if (X1 >= 0 && X1 <= 1)
                    return X1;
                else if (X2 >= 0 && X2 <= 1)
                    return X2;
                else
                    throw new ArgumentException();
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _path = (Path)GetTemplateChild(pathName);
            _thumbGrid = (Grid)GetTemplateChild(thumbGridName);
            _readingBlock = (TextBlock)GetTemplateChild(readingName);
            
            PathGeometry geo = new PathGeometry();
            _bfigure = new PathFigure();
            _bseg = new BezierSegment();
            _bfigure.Segments.Add(_bseg);
            geo.Figures.Add(_bfigure);
            _path.Data = geo;

            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSlider();
        }

        protected void UpdateSlider()
        {
            if (Orientation == Orientations.LeftToRight)
            {
                _bfigure.StartPoint = new Point(0, 0);
                _bseg.Point1 = new Point(0, 0);
                _bseg.Point2 = new Point(ActualWidth / 2, ActualHeight / 4);
                _bseg.Point3 = new Point(ActualWidth, ActualHeight);

                double t = bezierX2t(Value / 100 * ActualWidth);
                Point p = cubicBezier(t);
                _thumbGrid.Margin = new Thickness(p.X - ThumbSize / 2, p.Y - ThumbSize / 2, 0, 0);
                _readingBlock.Text = Value.ToString("0");
            }
            else
            {
                _bfigure.StartPoint = new Point(ActualWidth, 0);
                _bseg.Point1 = new Point(ActualWidth, 0);
                _bseg.Point2 = new Point(ActualWidth / 2, ActualHeight / 4);
                _bseg.Point3 = new Point(0, ActualHeight);

                double t = bezierX2t((1 - Value / 100) * ActualWidth);
                Point p = cubicBezier(t);
                _thumbGrid.Margin = new Thickness(p.X - ThumbSize / 2, p.Y - ThumbSize / 2, 0, 0);
                _readingBlock.Text = Value.ToString("0");
            }
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(this).Position;
            double old = Value;
            double _value = point.X * 100 / ActualWidth;
            if (Orientation == Orientations.RightToLeft) _value = 100 - _value;
            if (_value > 100) Value = 100; else if (_value < 0) Value = 0; else Value = _value;
            if (ValueChanged != null)
                ValueChanged(this, new SliderValueChangedEventArgs(old, Value, this));
            e.Handled = true;
        }
        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(this).Position;
            double old = Value;
            double _value = point.X * 100 / ActualWidth;
            if (Orientation == Orientations.RightToLeft) _value = 100 - _value;
            if (_value > 100) Value = 100; else if (_value < 0) Value = 0; else Value = _value;
            if (ValueChanged != null)
                ValueChanged(this, new SliderValueChangedEventArgs(old, Value, this));
            e.Handled = true;
        }
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(this).Position;
            double old = Value;
            double _value = point.X * 100 / ActualWidth;
            if (Orientation == Orientations.RightToLeft) _value = 100 - _value;
            if (_value > 100) Value = 100; else if (_value < 0) Value = 0; else Value = _value;
            if (ValueChanged != null)
                ValueChanged(this, new SliderValueChangedEventArgs(old, Value, this));
            CapturePointer(e.Pointer);
            e.Handled = true;
        }
        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            ReleasePointerCapture(e.Pointer);
            e.Handled = true;
        }
    }
}
