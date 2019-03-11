using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BuzzNet
{
    public partial class FormDiagnostics : Form
    {
        private double minimumAccuracy = -1.0;
        private double maximumAccuracy = -1.0;
        private double minimumError = -1.0;
        private double maximumError = -1.0;
        private int numDataPoints = 0;

        public FormDiagnostics()
        {
            InitializeComponent();
            chart1.ChartAreas[0].AxisX.Minimum = Properties.Settings.Default.IsLogarithmicXAxis ? 1 : 0; // purposefully > 0 for possible logarithmic x axis
            chart1.ChartAreas[0].AxisX2.Minimum = Properties.Settings.Default.IsLogarithmicXAxis ? 1 : 0; // purposefully > 0 for possible logarithmic x axis
            chart1.ChartAreas[0].AxisX2.IsLogarithmic = false;
            chart1.ChartAreas[0].AxisX.IsLogarithmic = false;
            chart1.ChartAreas[0].AxisX.Maximum = Properties.Settings.Default.MaxX;
            chart1.ChartAreas[0].AxisY.Maximum = (int)Properties.Settings.Default.MaximumError;
            chart1.ChartAreas[0].AxisY.Minimum = (int)Properties.Settings.Default.MaximumError - 1f;
            chart1.ChartAreas[0].AxisX2.Maximum = Properties.Settings.Default.MaxX;
            chart1.ChartAreas[0].AxisY2.Minimum = 99;
            chart1.ChartAreas[0].AxisY2.Maximum = 100;
        }

        delegate void AddDataDelegate(double error, double accuracy, ulong index);

        public void AddDataPoint(double error, double accuracy, ulong index)
        {
            if(InvokeRequired)
            {
                BeginInvoke(new AddDataDelegate(AddDataPoint), error, accuracy, index);
                return;
            }

            // logarithmic representation cannot deal with 0 and more that a billion datapoints are not supported
            if (index == 0 || index >= (ulong)Properties.Settings.Default.MaxX) return; 

            chart1.ChartAreas[0].AxisX2.IsLogarithmic = Properties.Settings.Default.IsLogarithmicXAxis;
            chart1.ChartAreas[0].AxisX.IsLogarithmic = Properties.Settings.Default.IsLogarithmicXAxis;

            numDataPoints++;

            if (minimumError < 0)
            {
                minimumError = error;
                maximumError = error;
                chart1.ChartAreas[0].AxisY.Maximum = maximumError;
                minimumAccuracy = accuracy;
                maximumAccuracy = accuracy;
                chart1.ChartAreas[0].AxisY.Minimum = (int)error - 10;
                chart1.ChartAreas[0].AxisY.Maximum = (int)error + 10;
                chart1.ChartAreas[0].AxisY2.Minimum = (int)accuracy - 1;
                //chart1.ChartAreas[0].AxisY2.Maximum = (int)accuracy + 1;
            }

            if (error < minimumError) minimumError = error;
            if (error > maximumError)
            {
                maximumError = error;
                chart1.ChartAreas[0].AxisY.Maximum = maximumError + 10;
            }

            if (accuracy < minimumAccuracy) minimumAccuracy = accuracy;
            if (accuracy > maximumAccuracy) maximumAccuracy = accuracy;

            if(!Properties.Settings.Default.IsLogarithmicXAxis)
            { 
                if (numDataPoints >= Properties.Settings.Default.MaxX)
                {
                    chart1.Series["Error"].Points.RemoveAt(0);
                    chart1.Series["Accuracy"].Points.RemoveAt(0);
                    chart1.ChartAreas[0].AxisX.Minimum = chart1.ChartAreas[0].AxisX.Minimum + 1;
                    chart1.ChartAreas[0].AxisX2.Minimum = chart1.ChartAreas[0].AxisX2.Minimum + 1;
                    chart1.ChartAreas[0].AxisX.Maximum = numDataPoints;
                    chart1.ChartAreas[0].AxisX2.Maximum = numDataPoints;
                    numDataPoints--;
                }
            }

            if ((int)error < chart1.ChartAreas[0].AxisY.Minimum) chart1.ChartAreas[0].AxisY.Minimum = (int)error - 10;
            if ((int)error > chart1.ChartAreas[0].AxisY.Maximum) chart1.ChartAreas[0].AxisY.Maximum = (int)error + 10;
            if ((int)accuracy < chart1.ChartAreas[0].AxisY2.Minimum) chart1.ChartAreas[0].AxisY2.Minimum = (int)accuracy - 1;
            //if ((int)accuracy > chart1.ChartAreas[0].AxisY2.Maximum) chart1.ChartAreas[0].AxisY2.Maximum = (int)accuracy + 1;

            if (numDataPoints >= (Properties.Settings.Default.DesiredNumOfChartDataPoints * Properties.Settings.Default.DataPointReduceFactor))
            {
                ReduceNumberOfDatapoints();
            }

            chart1.Series["Error"].Points.AddXY(index, error);
            chart1.Series["Accuracy"].Points.AddXY(index, accuracy);
            chart1.Series["MinimumError"].Points.AddXY(index, minimumError);
            chart1.Series["MaximumError"].Points.AddXY(index, maximumError);
            chart1.Series["MinimumAccuracy"].Points.AddXY(index, minimumAccuracy);
            chart1.Series["MaximumAccuracy"].Points.AddXY(index, maximumAccuracy);
        }

        private void ReduceNumberOfDatapoints()
        {
            // reduce the number of data points by factor 10
            List<DataPoint> lstError = new List<DataPoint>();
            List<DataPoint> lstMinError = new List<DataPoint>();
            List<DataPoint> lstMaxError = new List<DataPoint>();
            List<DataPoint> lstAcc = new List<DataPoint>();
            List<DataPoint> lstMinAcc = new List<DataPoint>();
            List<DataPoint> lstMaxAcc = new List<DataPoint>();
            int i = 0;
            foreach (DataPoint dp in chart1.Series["Error"].Points)
            {
                if ((i % Properties.Settings.Default.DesiredNumOfChartDataPoints) == 0)
                {
                    lstError.Add(dp.Clone());
                }
                i++;
            }
            i = 0;
            foreach (DataPoint dp in chart1.Series["MaximumError"].Points)
            {
                if ((i % Properties.Settings.Default.DesiredNumOfChartDataPoints) == 0)
                {
                    lstMaxError.Add(dp.Clone());
                }
                i++;
            }
            i = 0;
            foreach (DataPoint dp in chart1.Series["MinimumError"].Points)
            {
                if ((i % Properties.Settings.Default.DesiredNumOfChartDataPoints) == 0)
                {
                    lstMinError.Add(dp.Clone());
                }
                i++;
            }
            i = 0;
            foreach (DataPoint dp in chart1.Series["Accuracy"].Points)
            {
                if ((i % Properties.Settings.Default.DesiredNumOfChartDataPoints) == 0)
                {
                    lstAcc.Add(dp.Clone());
                }
                i++;
            }
            i = 0;
            foreach (DataPoint dp in chart1.Series["MaximumAccuracy"].Points)
            {
                if ((i % Properties.Settings.Default.DesiredNumOfChartDataPoints) == 0)
                {
                    lstMaxAcc.Add(dp.Clone());
                }
                i++;
            }
            i = 0;
            foreach (DataPoint dp in chart1.Series["MinimumAccuracy"].Points)
            {
                if ((i % Properties.Settings.Default.DesiredNumOfChartDataPoints) == 0)
                {
                    lstMinAcc.Add(dp.Clone());
                }
                i++;
            }

            chart1.Series["Error"].Points.Clear();
            chart1.Series["MaximumError"].Points.Clear();
            chart1.Series["MinimumError"].Points.Clear();
            chart1.Series["Accuracy"].Points.Clear();
            chart1.Series["MaximumAccuracy"].Points.Clear();
            chart1.Series["MinimumAccuracy"].Points.Clear();

            foreach (DataPoint dp in lstError)
            {
                chart1.Series["Error"].Points.AddXY((int)dp.XValue, dp.YValues[0]);
            }
            foreach (DataPoint dp in lstMaxError)
            {
                chart1.Series["MaximumError"].Points.AddXY((int)dp.XValue, dp.YValues[0]);
            }
            foreach (DataPoint dp in lstMinError)
            {
                chart1.Series["MinimumError"].Points.AddXY((int)dp.XValue, dp.YValues[0]);
            }
            foreach (DataPoint dp in lstAcc)
            {
                chart1.Series["Accuracy"].Points.AddXY((int)dp.XValue, dp.YValues[0]);
            }
            foreach (DataPoint dp in lstMaxAcc)
            {
                chart1.Series["MaximumAccuracy"].Points.AddXY((int)dp.XValue, dp.YValues[0]);
            }
            foreach (DataPoint dp in lstMinAcc)
            {
                chart1.Series["MinimumAccuracy"].Points.AddXY((int)dp.XValue, dp.YValues[0]);
            }
            numDataPoints = lstError.Count;
        }
    }
}
