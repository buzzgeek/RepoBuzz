namespace BuzzNet
{
    partial class FormDiagnostics
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series7 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series8 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series9 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series10 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series11 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series12 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDiagnostics));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.chart1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(707, 516);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(707, 541);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // chart1
            // 
            chartArea2.AxisY.LabelStyle.ForeColor = System.Drawing.Color.Maroon;
            chartArea2.AxisY2.LabelStyle.ForeColor = System.Drawing.Color.Green;
            chartArea2.AxisY2.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            chartArea2.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea2);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            legend2.Name = "Legend1";
            this.chart1.Legends.Add(legend2);
            this.chart1.Location = new System.Drawing.Point(0, 0);
            this.chart1.Name = "chart1";
            series7.ChartArea = "ChartArea1";
            series7.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series7.Color = System.Drawing.Color.Maroon;
            series7.Legend = "Legend1";
            series7.Name = "Error";
            series7.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.UInt64;
            series7.YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double;
            series8.BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.DashDotDot;
            series8.ChartArea = "ChartArea1";
            series8.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series8.Color = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            series8.Legend = "Legend1";
            series8.Name = "MinimumError";
            series8.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.UInt64;
            series8.YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double;
            series9.BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.DashDotDot;
            series9.ChartArea = "ChartArea1";
            series9.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series9.Color = System.Drawing.Color.Silver;
            series9.Legend = "Legend1";
            series9.Name = "MaximumError";
            series9.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.UInt64;
            series9.YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double;
            series10.ChartArea = "ChartArea1";
            series10.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series10.Color = System.Drawing.Color.Green;
            series10.Legend = "Legend1";
            series10.Name = "Accuracy";
            series10.XAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            series10.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.UInt64;
            series10.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            series10.YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double;
            series11.BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.DashDotDot;
            series11.ChartArea = "ChartArea1";
            series11.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series11.Color = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            series11.Legend = "Legend1";
            series11.Name = "MinimumAccuracy";
            series11.XAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            series11.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.UInt64;
            series11.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            series11.YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double;
            series12.BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.DashDotDot;
            series12.ChartArea = "ChartArea1";
            series12.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series12.Color = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            series12.Legend = "Legend1";
            series12.Name = "MaximumAccuracy";
            series12.XAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            series12.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.UInt64;
            series12.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            series12.YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double;
            this.chart1.Series.Add(series7);
            this.chart1.Series.Add(series8);
            this.chart1.Series.Add(series9);
            this.chart1.Series.Add(series10);
            this.chart1.Series.Add(series11);
            this.chart1.Series.Add(series12);
            this.chart1.Size = new System.Drawing.Size(707, 516);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            // 
            // FormDiagnostics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(707, 541);
            this.Controls.Add(this.toolStripContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormDiagnostics";
            this.Text = "Diagnostics";
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
    }
}