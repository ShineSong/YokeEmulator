using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CurveEditor
{
    public partial class MainForm : Form
    {
        Series seris;
        public MainForm()
        {
            InitializeComponent();
            seris = new Series("Spline");
            seris.ChartType = SeriesChartType.Line;
            seris.BorderWidth = 3;
        }

        private void AxisList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
