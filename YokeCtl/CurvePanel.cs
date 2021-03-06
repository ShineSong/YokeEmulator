﻿using System;
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
    public class CurvePanel : Panel
    {
        Canvas _bgcanvas = null;
        Path _path;
        BezierSegment _bseg = null;
        PathFigure _bfigure = null;
        double approxLength = 0;
        double tpadding = 0;
        public CurvePanel()
        {
            _bgcanvas = new Canvas();
            _bgcanvas.VerticalAlignment = VerticalAlignment.Stretch;
            _bgcanvas.HorizontalAlignment = HorizontalAlignment.Stretch;
            _path = new Path();
            _path.Stroke = Stroke;
            _path.StrokeThickness = StrokeThickness;
            PathGeometry geo = new PathGeometry();
            _bfigure = new PathFigure();
            _bseg = new BezierSegment();
            _bfigure.Segments.Add(_bseg);
            geo.Figures.Add(_bfigure);
            _path.Data = geo;
            _bgcanvas.Children.Add(_path);
            this.Children.Add(_bgcanvas);

            this.SizeChanged += OnSizeChanged;
        }
        #region Stroke
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(
                "Stroke",
                typeof(Brush),
                typeof(CurvePanel),
                new PropertyMetadata(new SolidColorBrush(Colors.White), OnStrokeChanged));
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }
        private static void OnStrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (CurvePanel)d;
            target._path.Stroke = (Brush)e.NewValue;
        }
        #endregion
        #region StrokeThickness
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                "StrokeThickness",
                typeof(double),
                typeof(CurvePanel),
                new PropertyMetadata(5d, OnStrokeThicknessChanged));
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        private static void OnStrokeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (CurvePanel)d;
            target._path.StrokeThickness = (double)e.NewValue;
        }
        #endregion
        #region ItemHeight
        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(
                "ItemHeight",
                typeof(double),
                typeof(CurvePanel),
                new PropertyMetadata(30d, OnPropertyChanged));
        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }
        #endregion

        #region Common Property Changed recall
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (CurvePanel)d;
            target.InvalidateArrange();
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
            double a = - 3 * _bseg.Point2.X + _bseg.Point3.X;
            double b = + 3 * _bseg.Point2.X;
            double c = 0;
            double d = - x;

            double A = b * b;
            double B =  - 9 * a * d;
            double C =  - 3 * b * d;
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
            } else if (delta < 0)
            {
                double theta = Math.Acos((2*A*b-3*a*B)/2/Math.Pow(A,1.5));
                double X1 = (-b - 2 * Math.Sqrt(A) * Math.Cos(theta / 3)) / 3 / a;
                double X2 = (-b + Math.Sqrt(A) * (Math.Cos(theta / 3) + Math.Sqrt(3) * Math.Sin(theta / 3))) / 3 / a;
                double X3 = (-b + Math.Sqrt(A) * (Math.Cos(theta / 3) - Math.Sqrt(3) * Math.Sin(theta / 3))) / 3 / a;
                if (X1 >= 0 && X1 <= 1)
                    return X1;
                else if (X2 >= 0 && X2 <= 1)
                    return X2;
                else if (X3 >= 0 && X3 <= 1)
                    return X3;
                else
                    throw new ArgumentException();
            }
            else {
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

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {

            _bfigure.StartPoint = new Point(0, 0);
            _bseg.Point1 = new Point(0, 0);
            _bseg.Point2 = new Point(ActualWidth / 2, ActualHeight / 4);
            _bseg.Point3 = new Point(ActualWidth, ActualHeight);
            approxLength = Math.Sqrt(Math.Pow(ActualHeight, 2) + Math.Pow(ActualWidth, 2));
            double elementLength = Math.Sqrt(2) * ItemHeight * 1.5;
            tpadding = elementLength / approxLength;

            this.InvalidateArrange();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement element in Children)
            {
                if (element.Equals(_bgcanvas))
                    continue;
                FrameworkElement _c = (FrameworkElement)element;
                _c.Width = ItemHeight;
                _c.Height = ItemHeight;
            }
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (ActualWidth == 0) return finalSize;
            foreach (UIElement element in Children)
            {
                if (element.Equals(_bgcanvas))
                    continue;
                FrameworkElement _f = (FrameworkElement)element;
                double pos = 0;
                try
                {
                    pos = double.Parse(_f.DataContext.ToString());
                }
                catch (Exception)
                {
                    pos = 0.5;
                }
                double t = bezierX2t(pos * ActualWidth);
                Point anchorPoint = cubicBezier(t);
                anchorPoint.X -= ItemHeight / 2;
                anchorPoint.Y -= ItemHeight / 2;
                element.Arrange(new Rect(anchorPoint, new Size(ItemHeight, ItemHeight)));
            }
            return finalSize;
        }

        
    }
}
