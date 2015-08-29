using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI;
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
    [TemplatePart(Name = canvasName,Type =typeof(Canvas))]
    [TemplatePart(Name = roundTriangeName, Type = typeof(Path))]
    [TemplatePart(Name = verticalTickName, Type = typeof(Canvas))]
    [TemplatePart(Name = arcTickName, Type = typeof(Canvas))]
    [TemplatePart(Name = rudderBtnName, Type = typeof(Ellipse))]
    [TemplatePart(Name = trackerBtnName, Type = typeof(Ellipse))]
    public sealed class RudderPad : Control
    {
        private const double DEG2RAD = Math.PI / 180.0;
        private const double RAD2DEG = 180.0 / Math.PI;

        private const string canvasName = "PART_Canvas";
        private const string roundTriangeName = "PART_RoundTriangle";
        private const string verticalTickName = "PART_VerticalTick";
        private const string arcTickName = "PART_ArcTick";
        private const string rudderBtnName = "PART_rudderBtn";
        private const string trackerBtnName="PART_trackerBtn";
        private Canvas _canvas = null;
        private Path _roundTriangle = null;
        private Canvas _verticalTick = null;
        private Canvas _arcTick = null;
        private Ellipse _rudderBtn = null;
        private Ellipse _trackerBtn = null;

        double _roundAreaLength;
        double _roundAreaBaseY;
        double _roundAreaControlLength;
        Point _slideCenter;
        public RudderPad()
        {
            this.DefaultStyleKey = typeof(RudderPad);
        }
        #region Stroke
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(
                "Stroke",
                typeof(Brush),
                typeof(RudderPad),
                new PropertyMetadata(new SolidColorBrush(Colors.White), OnStrokeChanged));
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }
        private static void OnStrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (RudderPad)d;
            if(target._roundTriangle != null)
                target._roundTriangle.Stroke = (Brush)e.NewValue;
        }
        #endregion
        #region StrokeThickness
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                "StrokeThickness",
                typeof(double),
                typeof(RudderPad),
                new PropertyMetadata(5d, OnStrokeThicknessChanged));
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        private static void OnStrokeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (RudderPad)d;
            if (target._roundTriangle != null)
                target._roundTriangle.StrokeThickness = (double)e.NewValue;
        }
        #endregion
        #region Value
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(double),
                typeof(RudderPad),
                new PropertyMetadata(0d, OnValueChanged));
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (RudderPad)d;
            if (target._rudderBtn != null)
                target.UpdateRudderButton();
        }
        public event SliderValueChangedEventHandler ValueChanged;
        #endregion
        #region RudderBtnSliderRadius
        public static readonly DependencyProperty RudderBtnSliderRadiusProperty =
            DependencyProperty.Register(
                "RudderBtnSliderRadius",
                typeof(double),
                typeof(RudderPad),
                new PropertyMetadata(0d, OnRudderBtnSliderRadiusChanged));
        public double RudderBtnSliderRadius
        {
            get { return (double)GetValue(RudderBtnSliderRadiusProperty); }
            set { SetValue(RudderBtnSliderRadiusProperty, value); }
        }
        private static void OnRudderBtnSliderRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (RudderPad)d;
        }
        #endregion
        #region Resilience
        public static readonly DependencyProperty ResilienceProperty =
            DependencyProperty.Register(
                "Resilience",
                typeof(bool),
                typeof(RudderPad),
                new PropertyMetadata(true));
        public bool Resilience
        {
            get { return (bool)GetValue(ResilienceProperty); }
            set { SetValue(ResilienceProperty, value); }
        }
        #endregion
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _canvas = (Canvas)GetTemplateChild(canvasName);
            _roundTriangle = (Path)GetTemplateChild(roundTriangeName);
            _verticalTick = (Canvas)GetTemplateChild(verticalTickName);
            _arcTick = (Canvas)GetTemplateChild(arcTickName);
            _rudderBtn = (Ellipse)GetTemplateChild(rudderBtnName);
            _trackerBtn = (Ellipse)GetTemplateChild(trackerBtnName);

            _canvas.SizeChanged += OnSizeChanged;
            _trackerBtn.PointerPressed += _trackerBtn_PointerPressed;
            _trackerBtn.PointerReleased += _trackerBtn_PointerReleased;
            _trackerBtn.PointerMoved += _trackerBtn_PointerMoved;
        }

        private void _trackerBtn_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        public event EventHandler TrackerBtnPressed;
        public event EventHandler TrackerBtnReleased;
        private void _trackerBtn_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _trackerBtn.CapturePointer(e.Pointer);
            if (TrackerBtnPressed != null)
                TrackerBtnPressed(this, EventArgs.Empty);
            e.Handled = true;
            _trackerBtn.Opacity = 1;
        }
        private void _trackerBtn_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _trackerBtn.ReleasePointerCapture(e.Pointer);
            if (TrackerBtnReleased != null)
                TrackerBtnReleased(this, EventArgs.Empty);
            e.Handled = true;
            _trackerBtn.Opacity = 0.5;
        }


        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            CapturePointer(e.Pointer);
            Point p = e.GetCurrentPoint(this).Position;
            double diffX = p.X - _slideCenter.X;
            double diffY = -(p.Y - _slideCenter.Y);
            double angle = Math.Atan2(diffX, diffY) * RAD2DEG;
            angle = (angle - 10) / 0.7;
            double old = Value;
            if (angle < 0) Value = 0; else if (angle > 100) Value = 100; else Value = angle;
            if (ValueChanged != null)
                ValueChanged(this, new SliderValueChangedEventArgs(Value, old, this));
            _rudderBtn.Opacity = 1;
        }
        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            base.OnPointerMoved(e);
            Point p = e.GetCurrentPoint(this).Position;
            double diffX = p.X - _slideCenter.X;
            double diffY = -(p.Y - _slideCenter.Y);
            double angle = Math.Atan2(diffX,diffY)*RAD2DEG;
            angle = (angle - 10) / 0.7;
            double old = Value;
            if (angle < 0) Value = 0; else if (angle > 100) Value = 100; else Value = angle;
            if (ValueChanged != null)
                ValueChanged(this, new SliderValueChangedEventArgs(Value, old, this));
        }
        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            _trackerBtn.ReleasePointerCapture(e.Pointer);
            if (Resilience)
            {
                double old = Value;
                Value = 50;
                if (ValueChanged != null)
                    ValueChanged(this, new SliderValueChangedEventArgs(Value, old, this));
            }
            _rudderBtn.Opacity = 0.7;
        }
        protected override void OnDoubleTapped(DoubleTappedRoutedEventArgs e)
        {
            base.OnDoubleTapped(e);
            double old = Value;
            Value = 50;
            if (ValueChanged != null)
                ValueChanged(this,new SliderValueChangedEventArgs(old,Value, this));
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            drawRoundTriangle();
            drawTickBar();
            drawRudderButton();
        }

        private void drawRudderButton()
        {
            _rudderBtn.Width = _roundAreaLength / 3;
            _rudderBtn.Height = _roundAreaLength / 3;
            double x = _slideCenter.X + RudderBtnSliderRadius * Math.Sin(45 * DEG2RAD);
            double y = _slideCenter.Y - RudderBtnSliderRadius * Math.Sin(45 * DEG2RAD);
            _rudderBtn.Margin = new Thickness(x - _rudderBtn.Width / 2, y - _rudderBtn.Height / 2, x + _rudderBtn.Width / 2, y + _rudderBtn.Height / 2);
        }

        private void UpdateRudderButton()
        {
            double angle = (Value * 0.7 + 10)*DEG2RAD;
            double x = _slideCenter.X + RudderBtnSliderRadius * Math.Sin(angle);
            double y = _slideCenter.Y - RudderBtnSliderRadius * Math.Cos(angle);
            _rudderBtn.Margin = new Thickness(x - _rudderBtn.Width / 2, y - _rudderBtn.Height / 2, x + _rudderBtn.Width / 2, y + _rudderBtn.Height / 2);
        }

        private void drawTickBar()
        {
            _verticalTick.Children.Clear();
            int verTickCount = 10;
            double verTickLength = _roundAreaLength / 8;
            double verTickHeight = _roundAreaLength * 0.33 / verTickCount;
            for (int i = 0; i < verTickCount; ++i)
            {
                Rectangle r = new Rectangle();
                r.Fill = Stroke;
                r.Width = verTickLength;
                r.Height = verTickHeight;
                r.RadiusX = r.Width / 5;
                r.RadiusY = r.Height / 5;
                double x = (ActualWidth-(1- Math.Sqrt(2) / 4) * _roundAreaLength) + i * verTickHeight * 2;
                double y = (1 - Math.Sqrt(2) / 4) * _roundAreaLength - i * verTickHeight * 2+_roundAreaBaseY;
                r.Margin = new Thickness(x- verTickLength / 2, y-verTickHeight/2, x+ verTickLength / 2, y+verTickHeight/2);
                RotateTransform rotate = new RotateTransform();
                rotate.Angle = 45;
                r.RenderTransform = rotate;
                _verticalTick.Children.Add(r);
            }
            _slideCenter = new Point(ActualWidth - _roundAreaLength, _roundAreaLength + _roundAreaBaseY);
            if (RudderBtnSliderRadius == 0)
                RudderBtnSliderRadius = Math.Sqrt(2) * _roundAreaLength / 2;

            double horTickLength = verTickLength;
            double horTickHeight = verTickHeight;
            int horTickCount = 8;
            for (int i = 0; i < horTickCount; i++)
            {
                double angle = Math.PI / 2 * i / horTickCount+5*DEG2RAD;
                Rectangle r = new Rectangle();
                r.Fill = Stroke;
                r.Width = horTickLength;
                r.Height = horTickHeight;
                r.RadiusX = r.Width / 5;
                r.RadiusY = r.Height / 5;
                double x = _slideCenter.X+Math.Sin(angle)*RudderBtnSliderRadius;
                double y = _slideCenter.Y-Math.Cos(angle)*RudderBtnSliderRadius;
                r.Margin = new Thickness(x+r.Width/2, y - r.Height/2,0,0);
                RotateTransform rotate = new RotateTransform();
                rotate.Angle = angle*RAD2DEG+90;
                r.RenderTransform = rotate;
                _arcTick.Children.Add(r);
            }
        }

        private void drawRoundTriangle()
        {
            _roundAreaLength = ActualHeight < ActualWidth ? ActualHeight : ActualWidth; // choose min one ,align right top.
            _roundAreaControlLength = _roundAreaLength / 3;
            _roundAreaBaseY = _roundAreaLength / 8;
            if (_roundAreaLength == 0) return;
            PathGeometry geo = new PathGeometry();
            PathFigure fig = new PathFigure();
            BezierSegment seg1 = new BezierSegment();
            BezierSegment seg2 = new BezierSegment();
            BezierSegment seg3 = new BezierSegment();

            fig.StartPoint = new Point(ActualWidth, _roundAreaBaseY);
            seg1.Point1 = new Point(ActualWidth+Math.Sin(45*DEG2RAD)*_roundAreaControlLength, Math.Cos(45 * DEG2RAD) * _roundAreaControlLength + _roundAreaBaseY);
            seg1.Point2 = new Point(ActualWidth - _roundAreaLength * Math.Tan(15 * DEG2RAD)+Math.Cos(15*DEG2RAD)*_roundAreaControlLength, _roundAreaLength-Math.Sin(15*DEG2RAD)* _roundAreaControlLength + _roundAreaBaseY);
            seg1.Point3 = new Point(ActualWidth - _roundAreaLength * Math.Tan(15 * DEG2RAD), _roundAreaLength + _roundAreaBaseY);

            seg2.Point1 = new Point(ActualWidth - _roundAreaLength * Math.Tan(15 * DEG2RAD) - Math.Cos(15 * DEG2RAD) * _roundAreaControlLength, _roundAreaLength + Math.Sin(15 * DEG2RAD) * _roundAreaControlLength + _roundAreaBaseY);
            seg2.Point2 = new Point(ActualWidth - _roundAreaLength - Math.Sin(15 * DEG2RAD) * _roundAreaControlLength, _roundAreaLength * Math.Tan(15 * DEG2RAD) + Math.Cos(15 * DEG2RAD) * _roundAreaControlLength + _roundAreaBaseY);
            seg2.Point3 = new Point(ActualWidth - _roundAreaLength, _roundAreaLength * Math.Tan(15 * DEG2RAD) + _roundAreaBaseY);

            seg3.Point1 = new Point(ActualWidth - _roundAreaLength + Math.Sin(15 * DEG2RAD) * _roundAreaControlLength, _roundAreaLength * Math.Tan(15 * DEG2RAD) - Math.Cos(15 * DEG2RAD) * _roundAreaControlLength + _roundAreaBaseY);
            seg3.Point2 = new Point(ActualWidth - Math.Sin(45 * DEG2RAD) * _roundAreaControlLength, -Math.Cos(45 * DEG2RAD) * _roundAreaControlLength + _roundAreaBaseY);
            seg3.Point3 = fig.StartPoint;
            

            fig.Segments.Add(seg1);
            fig.Segments.Add(seg2);
            fig.Segments.Add(seg3);
            geo.Figures.Add(fig);
            _roundTriangle.Data = geo;

            _trackerBtn.Width = _roundAreaLength / 3;
            _trackerBtn.Height = _roundAreaLength / 3;
            _trackerBtn.Margin = new Thickness(ActualWidth - _trackerBtn.Width * 2 / 3, _roundAreaBaseY - _trackerBtn.Height / 3, 0, 0);
        }
    }
}
