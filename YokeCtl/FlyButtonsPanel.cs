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
    [TemplatePart(Name = roundTriangeName, Type = typeof(Path))]
    [TemplatePart(Name = trackerBtnName, Type = typeof(Ellipse))]
    [TemplatePart(Name = btn1Name, Type = typeof(Ellipse))]
    [TemplatePart(Name = btn2Name, Type = typeof(Ellipse))]
    [TemplatePart(Name = btn3Name, Type = typeof(Ellipse))]
    [TemplatePart(Name = btn4Name, Type = typeof(Ellipse))]
    [TemplatePart(Name = btn5Name, Type = typeof(Ellipse))]
    [TemplatePart(Name = btn6Name, Type = typeof(Ellipse))]
    [TemplatePart(Name = btn7Name, Type = typeof(Ellipse))]
    [TemplatePart(Name = btn1TextName, Type = typeof(TextBlock))]
    [TemplatePart(Name = btn2TextName, Type = typeof(TextBlock))]
    [TemplatePart(Name = btn3TextName, Type = typeof(TextBlock))]
    [TemplatePart(Name = btn4TextName, Type = typeof(TextBlock))]
    [TemplatePart(Name = btn5TextName, Type = typeof(TextBlock))]
    [TemplatePart(Name = btn6TextName, Type = typeof(TextBlock))]
    [TemplatePart(Name = btn7TextName, Type = typeof(TextBlock))]
    [TemplatePart(Name = viewbox1Name, Type = typeof(Viewbox))]
    [TemplatePart(Name = viewbox2Name, Type = typeof(Viewbox))]
    [TemplatePart(Name = viewbox3Name, Type = typeof(Viewbox))]
    [TemplatePart(Name = viewbox4Name, Type = typeof(Viewbox))]
    [TemplatePart(Name = viewbox5Name, Type = typeof(Viewbox))]
    [TemplatePart(Name = viewbox6Name, Type = typeof(Viewbox))]
    [TemplatePart(Name = viewbox7Name, Type = typeof(Viewbox))]
    [TemplatePart(Name = btnGrid1Name, Type = typeof(Grid))]
    [TemplatePart(Name = btnGrid2Name, Type = typeof(Grid))]
    [TemplatePart(Name = btnGrid3Name, Type = typeof(Grid))]
    [TemplatePart(Name = btnGrid4Name, Type = typeof(Grid))]
    [TemplatePart(Name = btnGrid5Name, Type = typeof(Grid))]
    [TemplatePart(Name = btnGrid6Name, Type = typeof(Grid))]
    [TemplatePart(Name = btnGrid7Name, Type = typeof(Grid))]
    public sealed class FlyButtonsPanel : Control
    {
        private const double DEG2RAD = Math.PI / 180.0;
        private const double RAD2DEG = 180.0 / Math.PI;

        private const string canvasName = "PART_Canvas";
        private const string roundTriangeName = "PART_RoundTriangle";
        private const string trackerBtnName = "PART_trackerBtn";
        private const string btn1Name = "PART_btn1";
        private const string btn2Name = "PART_btn2";
        private const string btn3Name = "PART_btn3";
        private const string btn4Name = "PART_btn4";
        private const string btn5Name = "PART_btn5";
        private const string btn6Name = "PART_btn6";
        private const string btn7Name = "PART_btn7";
        private const string btn1TextName = "PART_btn1Text";
        private const string btn2TextName = "PART_btn2Text";
        private const string btn3TextName = "PART_btn3Text";
        private const string btn4TextName = "PART_btn4Text";
        private const string btn5TextName = "PART_btn5Text";
        private const string btn6TextName = "PART_btn6Text";
        private const string btn7TextName = "PART_btn7Text";
        private const string viewbox1Name = "PART_viewbox1";
        private const string viewbox2Name = "PART_viewbox2";
        private const string viewbox3Name = "PART_viewbox3";
        private const string viewbox4Name = "PART_viewbox4";
        private const string viewbox5Name = "PART_viewbox5";
        private const string viewbox6Name = "PART_viewbox6";
        private const string viewbox7Name = "PART_viewbox7";
        private const string btnGrid1Name = "PART_btnGrid1";
        private const string btnGrid2Name = "PART_btnGrid2";
        private const string btnGrid3Name = "PART_btnGrid3";
        private const string btnGrid4Name = "PART_btnGrid4";
        private const string btnGrid5Name = "PART_btnGrid5";
        private const string btnGrid6Name = "PART_btnGrid6";
        private const string btnGrid7Name = "PART_btnGrid7";

        private Canvas _canvas = null;
        private Path _roundTriangle = null;
        private Ellipse _trackerBtn = null;
        private Ellipse[] btns;
        private bool[] dtapped;
        private TextBlock[] btnTexts;
        private Viewbox[] viewboxs;
        private Grid[] btnGrids;


        private double _roundAreaLength;
        private double _roundAreaControlLength;
        private double _roundAreaBaseY;
        public FlyButtonsPanel()
        {
            this.DefaultStyleKey = typeof(FlyButtonsPanel);
        }

        public bool[] DoubleTappedProperty
        {
            get { return dtapped; }
            set {
                dtapped = value;
            }
        }

        #region Stroke
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(
                "Stroke",
                typeof(Brush),
                typeof(FlyButtonsPanel),
                new PropertyMetadata(new SolidColorBrush(Colors.White), OnStrokeChanged));
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }
        private static void OnStrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (FlyButtonsPanel)d;
        }
        #endregion
        #region StrokeThickness
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                "StrokeThickness",
                typeof(double),
                typeof(FlyButtonsPanel),
                new PropertyMetadata(5d, OnStrokeThicknessChanged));
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        private static void OnStrokeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (FlyButtonsPanel)d;
        }
        #endregion
        #region Btn1Text
        public static readonly DependencyProperty Btn1TextProperty =
            DependencyProperty.Register(
                "Btn1Text",
                typeof(string),
                typeof(FlyButtonsPanel),
                new PropertyMetadata("Btn1", OnBtn1TextChanged));
        public string Btn1Text
        {
            get { return (string)GetValue(Btn1TextProperty); }
            set { SetValue(Btn1TextProperty, value); }
        }
        private static void OnBtn1TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (FlyButtonsPanel)d;
            if (target.btnTexts != null)
                target.btnTexts[0].Text = (string)e.NewValue;
        }
        #endregion
        #region Btn2Text
        public static readonly DependencyProperty Btn2TextProperty =
            DependencyProperty.Register(
                "Btn2Text",
                typeof(string),
                typeof(FlyButtonsPanel),
                new PropertyMetadata("Btn2", OnBtn2TextChanged));
        public string Btn2Text
        {
            get { return (string)GetValue(Btn2TextProperty); }
            set { SetValue(Btn2TextProperty, value); }
        }
        private static void OnBtn2TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (FlyButtonsPanel)d;
            if (target.btnTexts != null)
                target.btnTexts[1].Text = (string)e.NewValue;
        }
        #endregion
        #region Btn3Text
        public static readonly DependencyProperty Btn3TextProperty =
            DependencyProperty.Register(
                "Btn3Text",
                typeof(string),
                typeof(FlyButtonsPanel),
                new PropertyMetadata("Btn3", OnBtn3TextChanged));
        public string Btn3Text
        {
            get { return (string)GetValue(Btn3TextProperty); }
            set { SetValue(Btn3TextProperty, value); }
        }
        private static void OnBtn3TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (FlyButtonsPanel)d;
            if (target.btnTexts != null)
                target.btnTexts[2].Text = (string)e.NewValue;
        }
        #endregion
        #region Btn4Text
        public static readonly DependencyProperty Btn4TextProperty =
            DependencyProperty.Register(
                "Btn4Text",
                typeof(string),
                typeof(FlyButtonsPanel),
                new PropertyMetadata("Btn4", OnBtn4TextChanged));
        public string Btn4Text
        {
            get { return (string)GetValue(Btn4TextProperty); }
            set { SetValue(Btn4TextProperty, value); }
        }
        private static void OnBtn4TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (FlyButtonsPanel)d;
            if (target.btnTexts != null)
                target.btnTexts[3].Text = (string)e.NewValue;
        }
        #endregion
        #region Btn5Text
        public static readonly DependencyProperty Btn5TextProperty =
            DependencyProperty.Register(
                "Btn5Text",
                typeof(string),
                typeof(FlyButtonsPanel),
                new PropertyMetadata("Btn5", OnBtn5TextChanged));
        public string Btn5Text
        {
            get { return (string)GetValue(Btn5TextProperty); }
            set { SetValue(Btn5TextProperty, value); }
        }
        private static void OnBtn5TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (FlyButtonsPanel)d;
            if (target.btnTexts != null)
                target.btnTexts[4].Text = (string)e.NewValue;
        }
        #endregion
        #region Btn6Text
        public static readonly DependencyProperty Btn6TextProperty =
            DependencyProperty.Register(
                "Btn6Text",
                typeof(string),
                typeof(FlyButtonsPanel),
                new PropertyMetadata("Btn6", OnBtn6TextChanged));
        public string Btn6Text
        {
            get { return (string)GetValue(Btn6TextProperty); }
            set { SetValue(Btn6TextProperty, value); }
        }
        private static void OnBtn6TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (FlyButtonsPanel)d;
            if (target.btnTexts != null)
                target.btnTexts[5].Text = (string)e.NewValue;
        }
        #endregion
        #region Btn7Text
        public static readonly DependencyProperty Btn7TextProperty =
            DependencyProperty.Register(
                "Btn7Text",
                typeof(string),
                typeof(FlyButtonsPanel),
                new PropertyMetadata("Btn7", OnBtn7TextChanged));
        

        public string Btn7Text
        {
            get { return (string)GetValue(Btn7TextProperty); }
            set { SetValue(Btn7TextProperty, value); }
        }

        private static void OnBtn7TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (FlyButtonsPanel)d;
            if (target.btnTexts != null)
                target.btnTexts[6].Text = (string)e.NewValue;
        }
        #endregion

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _canvas = (Canvas)GetTemplateChild(canvasName);
            _roundTriangle = (Path)GetTemplateChild(roundTriangeName);
            _trackerBtn = (Ellipse)GetTemplateChild(trackerBtnName);
            btns = new Ellipse[7];
            btns[0] = (Ellipse)GetTemplateChild(btn1Name);
            btns[1] = (Ellipse)GetTemplateChild(btn2Name);
            btns[2] = (Ellipse)GetTemplateChild(btn3Name);
            btns[3] = (Ellipse)GetTemplateChild(btn4Name);
            btns[4] = (Ellipse)GetTemplateChild(btn5Name);
            btns[5] = (Ellipse)GetTemplateChild(btn6Name);
            btns[6] = (Ellipse)GetTemplateChild(btn7Name);
            btnTexts = new TextBlock[7];
            btnTexts[0] = (TextBlock)GetTemplateChild(btn1TextName);
            btnTexts[1] = (TextBlock)GetTemplateChild(btn2TextName);
            btnTexts[2] = (TextBlock)GetTemplateChild(btn3TextName);
            btnTexts[3] = (TextBlock)GetTemplateChild(btn4TextName);
            btnTexts[4] = (TextBlock)GetTemplateChild(btn5TextName);
            btnTexts[5] = (TextBlock)GetTemplateChild(btn6TextName);
            btnTexts[6] = (TextBlock)GetTemplateChild(btn7TextName);
            viewboxs = new Viewbox[7];
            viewboxs[0] = (Viewbox)GetTemplateChild(viewbox1Name);
            viewboxs[1] = (Viewbox)GetTemplateChild(viewbox2Name);
            viewboxs[2] = (Viewbox)GetTemplateChild(viewbox3Name);
            viewboxs[3] = (Viewbox)GetTemplateChild(viewbox4Name);
            viewboxs[4] = (Viewbox)GetTemplateChild(viewbox5Name);
            viewboxs[5] = (Viewbox)GetTemplateChild(viewbox6Name);
            viewboxs[6] = (Viewbox)GetTemplateChild(viewbox7Name);
            btnGrids = new Grid[7];
            btnGrids[0] = (Grid)GetTemplateChild(btnGrid1Name);
            btnGrids[1] = (Grid)GetTemplateChild(btnGrid2Name);
            btnGrids[2] = (Grid)GetTemplateChild(btnGrid3Name);
            btnGrids[3] = (Grid)GetTemplateChild(btnGrid4Name);
            btnGrids[4] = (Grid)GetTemplateChild(btnGrid5Name);
            btnGrids[5] = (Grid)GetTemplateChild(btnGrid6Name);
            btnGrids[6] = (Grid)GetTemplateChild(btnGrid7Name);
            _canvas.SizeChanged += OnSizeChanged;
            _trackerBtn.PointerPressed += _trackerBtn_PointerPressed;
            _trackerBtn.PointerReleased += _trackerBtn_PointerReleased;
            _trackerBtn.PointerMoved += _trackerBtn_PointerMoved;
            for(int i=0;i<7;++i)
            {
                btns[i].PointerPressed += btns_PointerPressed;
                btns[i].PointerReleased += btns_PointerReleased;
                btns[i].DoubleTapped += FlyButtonsPanel_DoubleTapped;
                btns[i].Opacity = dtapped[i] ? 1 : 0.5;
            }
        }

        #region buttons
        public event ButtonsPanelEventHandler ButtonsPressed;
        public event ButtonsPanelEventHandler ButtonsReleased;
        private void FlyButtonsPanel_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            int id = Array.IndexOf(btns, sender);
            dtapped[id] = true;
        }
        private void btns_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Ellipse btn = (Ellipse)sender;
            btn.CapturePointer(e.Pointer);
            int idx = Array.IndexOf(btns, sender);
            if (ButtonsPressed != null)
                ButtonsPressed(this, new ButtonsPanelEventArgs(idx + 1, ButtonsPanelEventArgs.State.Pressed));
            e.Handled = true;
            btn.Opacity = 1;
            dtapped[idx] = false;
        }

        private void btns_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Ellipse btn = (Ellipse)sender;
            btn.ReleasePointerCapture(e.Pointer);
            int idx = Array.IndexOf(btns, sender);
            if (dtapped[idx])
            {
                if (ButtonsPressed != null)
                    ButtonsPressed(this, new ButtonsPanelEventArgs(idx + 1, ButtonsPanelEventArgs.State.Pressed));
                btn.Opacity = 1;
            }
            else
            {
                if (ButtonsReleased != null)
                    ButtonsReleased(this, new ButtonsPanelEventArgs(idx + 1, ButtonsPanelEventArgs.State.Released));
                btn.Opacity = 0.5;
            }
            e.Handled=true;
        }
        #endregion

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            drawRoundTriangle();
            drawButtons();
        }

        private void drawButtons()
        {
            double[] btnSize=new double[7];
            btnSize[0] = _roundAreaLength / 2.7;
            btnSize[1] = _roundAreaLength / 4;
            btnSize[2] = _roundAreaLength / 3;
            btnSize[3] = _roundAreaLength / 4;
            btnSize[4] = _roundAreaLength / 4;
            btnSize[5] = _roundAreaLength / 3;
            btnSize[6] = _roundAreaLength / 4;
            string[] btnTextString = new string[7];
            btnTextString[0] = Btn1Text;
            btnTextString[1] = Btn2Text;
            btnTextString[2] = Btn3Text;
            btnTextString[3] = Btn4Text;
            btnTextString[4] = Btn5Text;
            btnTextString[5] = Btn6Text;
            btnTextString[6] = Btn7Text;
            double radius = _roundAreaLength/10;
            for (int i = 0; i < 7; ++i)
            {

                double x = Math.Cos((60 * i + 15) * DEG2RAD) * (radius+btnSize[i]) + _roundAreaLength / 2;
                double y = Math.Sin((60 * i + 15) * DEG2RAD) * (radius+btnSize[i]) + _roundAreaLength / 2+_roundAreaBaseY;
                btns[i].Width = btnSize[i];
                btns[i].Height = btnSize[i];
                if (i == 0)
                {
                    btns[i].Margin = new Thickness(_roundAreaLength / 2 - btns[i].Width / 2, _roundAreaLength / 2 - btns[i].Height / 2+ _roundAreaBaseY, _roundAreaLength / 2 + btns[i].Width / 2, _roundAreaLength / 2 + btns[i].Height / 2 + _roundAreaBaseY);
                }
                else
                {
                    btns[i].Margin = new Thickness(x - btns[i].Width / 2, y - btns[i].Height / 2, x + btns[i].Width / 2, y + btns[i].Height / 2);
                }
                btns[i].Stroke = Stroke;
                btns[i].StrokeThickness = StrokeThickness;
                                
                btnTexts[i].Text = btnTextString[i];

                viewboxs[i].Width = btnSize[i];
                viewboxs[i].Height = btnSize[i];
                viewboxs[i].Margin = btns[i].Margin;
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

            fig.StartPoint = new Point(0, _roundAreaBaseY);
            seg1.Point1 = new Point(- Math.Sin(45 * DEG2RAD) * _roundAreaControlLength, Math.Cos(45 * DEG2RAD) * _roundAreaControlLength + _roundAreaBaseY);
            seg1.Point2 = new Point(_roundAreaLength * Math.Tan(15 * DEG2RAD) - Math.Cos(15 * DEG2RAD) * _roundAreaControlLength, _roundAreaLength - Math.Sin(15 * DEG2RAD) * _roundAreaControlLength + _roundAreaBaseY);
            seg1.Point3 = new Point(_roundAreaLength * Math.Tan(15 * DEG2RAD), _roundAreaLength + _roundAreaBaseY);

            seg2.Point1 = new Point(_roundAreaLength * Math.Tan(15 * DEG2RAD) + Math.Cos(15 * DEG2RAD) * _roundAreaControlLength, _roundAreaLength + Math.Sin(15 * DEG2RAD) * _roundAreaControlLength + _roundAreaBaseY);
            seg2.Point2 = new Point(_roundAreaLength + Math.Sin(15 * DEG2RAD) * _roundAreaControlLength, _roundAreaLength * Math.Tan(15 * DEG2RAD) + Math.Cos(15 * DEG2RAD) * _roundAreaControlLength + _roundAreaBaseY);
            seg2.Point3 = new Point(_roundAreaLength, _roundAreaLength * Math.Tan(15 * DEG2RAD) + _roundAreaBaseY);

            seg3.Point1 = new Point(_roundAreaLength - Math.Sin(15 * DEG2RAD) * _roundAreaControlLength, _roundAreaLength * Math.Tan(15 * DEG2RAD) - Math.Cos(15 * DEG2RAD) * _roundAreaControlLength + _roundAreaBaseY);
            seg3.Point2 = new Point(Math.Sin(45 * DEG2RAD) * _roundAreaControlLength, -Math.Cos(45 * DEG2RAD) * _roundAreaControlLength + _roundAreaBaseY);
            seg3.Point3 = fig.StartPoint;


            fig.Segments.Add(seg1);
            fig.Segments.Add(seg2);
            fig.Segments.Add(seg3);
            geo.Figures.Add(fig);
            _roundTriangle.Data = geo;

            _trackerBtn.Width = _roundAreaLength / 3;
            _trackerBtn.Height = _roundAreaLength / 3;
            _trackerBtn.Margin = new Thickness(-_trackerBtn.Width * 1 / 3, _roundAreaBaseY - _trackerBtn.Height / 3, 0, 0);
        }

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
