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
    [TemplatePart(Name = canvasName, Type = typeof(Canvas))]
    [TemplatePart(Name = roundButton1Name, Type = typeof(Path))]
    [TemplatePart(Name = roundButton2Name, Type = typeof(Path))]
    [TemplatePart(Name = trackerBtnName, Type = typeof(Ellipse))]
    [TemplatePart(Name = btn1Name, Type = typeof(Ellipse))]
    [TemplatePart(Name = btn2Name, Type = typeof(Ellipse))]
    [TemplatePart(Name = btn1TextName, Type = typeof(TextBlock))]
    [TemplatePart(Name = btn2TextName, Type = typeof(TextBlock))]
    [TemplatePart(Name = viewbox1Name, Type = typeof(Viewbox))]
    [TemplatePart(Name = viewbox2Name, Type = typeof(Viewbox))]
    [TemplatePart(Name = btnGrid1Name, Type = typeof(Grid))]
    [TemplatePart(Name = btnGrid2Name, Type = typeof(Grid))]
    public sealed class BattleButtonsPanel : Control
    {
        private const double DEG2RAD = Math.PI / 180.0;
        private const double RAD2DEG = 180.0 / Math.PI;

        private const string canvasName = "PART_Canvas";
        private const string roundButton1Name = "PART_RoundButton1";
        private const string roundButton2Name = "PART_RoundButton2";
        private const string trackerBtnName = "PART_trackerBtn";
        private const string btn1Name = "PART_btn1";
        private const string btn2Name = "PART_btn2";
        private const string btn1TextName = "PART_btn1Text";
        private const string btn2TextName = "PART_btn2Text";
        private const string viewbox1Name = "PART_viewbox1";
        private const string viewbox2Name = "PART_viewbox2";
        private const string btnGrid1Name = "PART_btnGrid1";
        private const string btnGrid2Name = "PART_btnGrid2";

        private Canvas _canvas = null;
        private Path _roundButton1 = null;
        private Path _roundButton2 = null;
        private Ellipse _trackerBtn = null;
        private bool[] dtapped;
        private TextBlock[] btnTexts;
        private Viewbox[] viewboxs;
        private Grid[] btnGrids;

        private double _roundAreaLength1;
        private double _roundAreaControlLength1;
        private double _roundAreaBaseY1;

        private double _roundAreaLength2;
        private double _roundAreaControlLength2;
        private double _roundAreaBaseY2;
        public BattleButtonsPanel()
        {
            this.DefaultStyleKey = typeof(BattleButtonsPanel);
        }
        public bool[] DoubleTappedProperty
        {
            get { return dtapped; }
            set
            {
                dtapped = value;
            }
        }
        #region Stroke
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(
                "Stroke",
                typeof(Brush),
                typeof(BattleButtonsPanel),
                new PropertyMetadata(new SolidColorBrush(Colors.White), OnStrokeChanged));
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }
        private static void OnStrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (BattleButtonsPanel)d;
        }
        #endregion
        #region StrokeThickness
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                "StrokeThickness",
                typeof(double),
                typeof(BattleButtonsPanel),
                new PropertyMetadata(5d, OnStrokeThicknessChanged));
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        private static void OnStrokeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (BattleButtonsPanel)d;
            target._roundButton1.StrokeThickness = (double)e.NewValue;
            target._roundButton2.StrokeThickness = (double)e.NewValue;
        }
        #endregion
        #region Btn1Text
        public static readonly DependencyProperty Btn1TextProperty =
            DependencyProperty.Register(
                "Btn1Text",
                typeof(string),
                typeof(BattleButtonsPanel),
                new PropertyMetadata("Btn1", OnBtn1TextChanged));
        public string Btn1Text
        {
            get { return (string)GetValue(Btn1TextProperty); }
            set { SetValue(Btn1TextProperty, value); }
        }
        private static void OnBtn1TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (BattleButtonsPanel)d;
            if (target.btnTexts != null)
                target.btnTexts[0].Text = e.NewValue.ToString();
        }
        #endregion
        #region Btn2Text
        public static readonly DependencyProperty Btn2TextProperty =
            DependencyProperty.Register(
                "Btn2Text",
                typeof(string),
                typeof(BattleButtonsPanel),
                new PropertyMetadata("Btn2", OnBtn2TextChanged));
        public string Btn2Text
        {
            get { return (string)GetValue(Btn2TextProperty); }
            set { SetValue(Btn2TextProperty, value); }
        }
        private static void OnBtn2TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (BattleButtonsPanel)d;
            if (target.btnTexts != null)
                target.btnTexts[1].Text = e.NewValue.ToString();
        }
        #endregion
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _canvas = (Canvas)GetTemplateChild(canvasName);
            _roundButton1 = (Path)GetTemplateChild(roundButton1Name);
            _roundButton2 = (Path)GetTemplateChild(roundButton2Name);
            _trackerBtn = (Ellipse)GetTemplateChild(trackerBtnName);

            btnTexts = new TextBlock[2];
            btnTexts[0] = (TextBlock)GetTemplateChild(btn1TextName);
            btnTexts[1] = (TextBlock)GetTemplateChild(btn2TextName);
            btnGrids = new Grid[2];
            btnGrids[0] = (Grid)GetTemplateChild(btnGrid1Name);
            btnGrids[1] = (Grid)GetTemplateChild(btnGrid2Name);
            viewboxs = new Viewbox[2];
            viewboxs[0] = (Viewbox)GetTemplateChild(viewbox1Name);
            viewboxs[1] = (Viewbox)GetTemplateChild(viewbox2Name);
            btnTexts[0].Text = Btn1Text;
            btnTexts[1].Text = Btn2Text;
            if(dtapped[0])
                btnTexts[0].FontWeight = Windows.UI.Text.FontWeights.ExtraBold;
            if (dtapped[1])
                btnTexts[1].FontWeight = Windows.UI.Text.FontWeights.ExtraBold;

            _canvas.SizeChanged += OnSizeChanged;
            _trackerBtn.PointerPressed += _trackerBtn_PointerPressed;
            _trackerBtn.PointerReleased += _trackerBtn_PointerReleased;
            _trackerBtn.PointerMoved += _trackerBtn_PointerMoved;
            _roundButton1.PointerPressed += btns_PointerPressed;
            _roundButton1.PointerReleased += btns_PointerReleased;
            _roundButton1.DoubleTapped += btns_DoubleTapped;
            _roundButton2.PointerPressed += btns_PointerPressed;
            _roundButton2.PointerReleased += btns_PointerReleased;
            _roundButton2.DoubleTapped += btns_DoubleTapped;
        }


        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            drawRoundTriangleButtons();
        }

        private void drawRoundTriangleButtons()
        {
            //button large
            _roundAreaLength1 = ActualHeight < ActualWidth ? ActualHeight : ActualWidth; // choose min one ,align right top.
            _roundAreaControlLength1 = _roundAreaLength1 / 3;
            _roundAreaBaseY1 = _roundAreaLength1 / 8;
            if (_roundAreaLength1 == 0) return;
            PathGeometry geo = new PathGeometry();
            PathFigure fig = new PathFigure();
            BezierSegment seg1 = new BezierSegment();
            BezierSegment seg2 = new BezierSegment();
            BezierSegment seg3 = new BezierSegment();

            fig.StartPoint = new Point(0, _roundAreaBaseY1);
            seg1.Point1 = new Point(-Math.Sin(45 * DEG2RAD) * _roundAreaControlLength1, Math.Cos(45 * DEG2RAD) * _roundAreaControlLength1 + _roundAreaBaseY1);
            seg1.Point2 = new Point(_roundAreaLength1 * Math.Tan(15 * DEG2RAD) - Math.Cos(15 * DEG2RAD) * _roundAreaControlLength1, _roundAreaLength1 - Math.Sin(15 * DEG2RAD) * _roundAreaControlLength1 + _roundAreaBaseY1);
            seg1.Point3 = new Point(_roundAreaLength1 * Math.Tan(15 * DEG2RAD), _roundAreaLength1 + _roundAreaBaseY1);

            seg2.Point1 = new Point(_roundAreaLength1 * Math.Tan(15 * DEG2RAD) + Math.Cos(15 * DEG2RAD) * _roundAreaControlLength1, _roundAreaLength1 + Math.Sin(15 * DEG2RAD) * _roundAreaControlLength1 + _roundAreaBaseY1);
            seg2.Point2 = new Point(_roundAreaLength1 + Math.Sin(15 * DEG2RAD) * _roundAreaControlLength1, _roundAreaLength1 * Math.Tan(15 * DEG2RAD) + Math.Cos(15 * DEG2RAD) * _roundAreaControlLength1 + _roundAreaBaseY1);
            seg2.Point3 = new Point(_roundAreaLength1, _roundAreaLength1 * Math.Tan(15 * DEG2RAD) + _roundAreaBaseY1);

            seg3.Point1 = new Point(_roundAreaLength1 - Math.Sin(15 * DEG2RAD) * _roundAreaControlLength1, _roundAreaLength1 * Math.Tan(15 * DEG2RAD) - Math.Cos(15 * DEG2RAD) * _roundAreaControlLength1 + _roundAreaBaseY1);
            seg3.Point2 = new Point(Math.Sin(45 * DEG2RAD) * _roundAreaControlLength1, -Math.Cos(45 * DEG2RAD) * _roundAreaControlLength1 + _roundAreaBaseY1);
            seg3.Point3 = fig.StartPoint;

            fig.Segments.Add(seg1);
            fig.Segments.Add(seg2);
            fig.Segments.Add(seg3);
            geo.Figures.Add(fig);
            _roundButton1.Data = geo;
            viewboxs[0].Width = _roundAreaLength1*9/10;
            viewboxs[0].Height = _roundAreaLength1;
            viewboxs[1].Margin = new Thickness(0, _roundAreaBaseY1 / 2, 0, 0);
            btnTexts[0].FontSize = _roundAreaLength1 / 10;

            //button small
            _roundAreaLength2 = _roundAreaLength1 / 3; // choose min one ,align right top.
            _roundAreaControlLength2 = _roundAreaLength2 / 3;
            _roundAreaBaseY2 = _roundAreaLength2 / 8;
            if (_roundAreaLength1 == 0) return;
            PathGeometry geo2 = new PathGeometry();
            PathFigure fig2 = new PathFigure();
            BezierSegment seg21 = new BezierSegment();
            BezierSegment seg22 = new BezierSegment();
            BezierSegment seg23 = new BezierSegment();

            fig2.StartPoint = new Point(ActualWidth, _roundAreaBaseY2);
            seg21.Point1 = new Point(ActualWidth + Math.Sin(45 * DEG2RAD) * _roundAreaControlLength2, Math.Cos(45 * DEG2RAD) * _roundAreaControlLength2 + _roundAreaBaseY2);
            seg21.Point2 = new Point(ActualWidth - _roundAreaLength2 * Math.Tan(15 * DEG2RAD) + Math.Cos(15 * DEG2RAD) * _roundAreaControlLength2, _roundAreaLength2 - Math.Sin(15 * DEG2RAD) * _roundAreaControlLength2 + _roundAreaBaseY2);
            seg21.Point3 = new Point(ActualWidth - _roundAreaLength2 * Math.Tan(15 * DEG2RAD), _roundAreaLength2 + _roundAreaBaseY2);

            seg22.Point1 = new Point(ActualWidth - _roundAreaLength2 * Math.Tan(15 * DEG2RAD) - Math.Cos(15 * DEG2RAD) * _roundAreaControlLength2, _roundAreaLength2 + Math.Sin(15 * DEG2RAD) * _roundAreaControlLength2 + _roundAreaBaseY2);
            seg22.Point2 = new Point(ActualWidth - _roundAreaLength2 - Math.Sin(15 * DEG2RAD) * _roundAreaControlLength2, _roundAreaLength2 * Math.Tan(15 * DEG2RAD) + Math.Cos(15 * DEG2RAD) * _roundAreaControlLength2 + _roundAreaBaseY2);
            seg22.Point3 = new Point(ActualWidth - _roundAreaLength2, _roundAreaLength2 * Math.Tan(15 * DEG2RAD) + _roundAreaBaseY2);

            seg23.Point1 = new Point(ActualWidth - _roundAreaLength2 + Math.Sin(15 * DEG2RAD) * _roundAreaControlLength2, _roundAreaLength2 * Math.Tan(15 * DEG2RAD) - Math.Cos(15 * DEG2RAD) * _roundAreaControlLength2 + _roundAreaBaseY2);
            seg23.Point2 = new Point(ActualWidth - Math.Sin(45 * DEG2RAD) * _roundAreaControlLength2, -Math.Cos(45 * DEG2RAD) * _roundAreaControlLength2 + _roundAreaBaseY2);
            seg23.Point3 = fig2.StartPoint;

            fig2.Segments.Add(seg21);
            fig2.Segments.Add(seg22);
            fig2.Segments.Add(seg23);
            geo2.Figures.Add(fig2);
            _roundButton2.Data = geo2;
            viewboxs[1].Width = _roundAreaLength2;
            viewboxs[1].Height = _roundAreaLength2;
            viewboxs[1].Margin = new Thickness(ActualWidth-_roundAreaLength2*8/9,_roundAreaBaseY2/2,0,0);
            btnTexts[1].FontSize = _roundAreaLength2 / 4;

            _trackerBtn.Width = _roundAreaLength1 / 3;
            _trackerBtn.Height = _roundAreaLength1 / 3;
            _trackerBtn.Margin = new Thickness(-_trackerBtn.Width * 1 / 3, _roundAreaBaseY1 - _trackerBtn.Height / 3, 0, 0);
        }
        #region buttons
        public event ButtonsPanelEventHandler ButtonsPressed;
        public event ButtonsPanelEventHandler ButtonsReleased;
        private void btns_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            int idx = (sender == _roundButton1) ? 0 : 1;
            dtapped[idx] = true;
        }
        private void btns_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            UIElement btn = (UIElement)sender;
            btn.CapturePointer(e.Pointer);
            int idx = (sender == _roundButton1) ? 0 : 1;
            if (ButtonsPressed != null)
                ButtonsPressed(this, new ButtonsPanelEventArgs(idx + 1, ButtonsPanelEventArgs.State.Pressed));
            e.Handled = true;
            btnTexts[idx].FontWeight = Windows.UI.Text.FontWeights.ExtraBold;
            dtapped[idx] = false;
        }

        private void btns_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            UIElement btn = (UIElement)sender;
            btn.ReleasePointerCapture(e.Pointer);
            int idx = (sender == _roundButton1) ? 0 : 1;
            if (dtapped[idx])
            {
                if (ButtonsPressed != null)
                    ButtonsPressed(this, new ButtonsPanelEventArgs(idx + 1, ButtonsPanelEventArgs.State.Pressed));
                btnTexts[idx].FontWeight = Windows.UI.Text.FontWeights.ExtraBold;
            }
            else
            {
                if (ButtonsReleased != null)
                    ButtonsReleased(this, new ButtonsPanelEventArgs(idx + 1, ButtonsPanelEventArgs.State.Released));
                btnTexts[idx].FontWeight = Windows.UI.Text.FontWeights.Normal;
            }
            e.Handled = true;
        }
        #endregion
        #region Tracker
        public event EventHandler TrackerBtnPressed;
        public event EventHandler TrackerBtnReleased;
        public event PointerEventHandler TrackerBtnMoved;
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

        private void _trackerBtn_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (TrackerBtnMoved != null)
                TrackerBtnMoved(this, e);
            e.Handled = true;
        }
        #endregion
    }
}
