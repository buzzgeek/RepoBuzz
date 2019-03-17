using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

/// <summary>
/// Copyright (c) 2019 Buzz-Barry Struck
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.
/// </summary>

namespace BuzzNet
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        //private System.ComponentModel.Container components = null;
        private System.Windows.Forms.ToolBar toolBar1;
        private System.Windows.Forms.ToolBarButton toolBarButtonStart;
        private System.Windows.Forms.ToolBarButton toolBarButtonTest;
        private System.Windows.Forms.ToolBarButton toolBarButton1;
        private System.Windows.Forms.ToolBarButton toolBarButton2;
        private System.Windows.Forms.ToolBarButton toolBarButton3;
        private System.Windows.Forms.ToolBarButton toolBarButton4;
        private System.Windows.Forms.ToolBarButton toolBarButton5;
        private TableLayoutPanel tableLayoutPanel1;
        private ToolStripContainer toolStripContainer1;
        private TestControl testControl1;
        //private FormDiagnostics diagnostics = new FormDiagnostics();

        public Form1()
        {
            InitializeComponent();
            this.Text = string.Format("{0} - {1}", this.Text, Process.GetCurrentProcess().Id);

            this.Size = new Size(300, 320);
            //this.diagnostics.Show(this);

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.testControl1 = new BuzzNet.TestControl();
            this.toolBar1 = new System.Windows.Forms.ToolBar();
            this.toolBarButtonStart = new System.Windows.Forms.ToolBarButton();
            this.toolBarButtonTest = new System.Windows.Forms.ToolBarButton();
            this.toolBarButton1 = new System.Windows.Forms.ToolBarButton();
            this.toolBarButton2 = new System.Windows.Forms.ToolBarButton();
            this.toolBarButton3 = new System.Windows.Forms.ToolBarButton();
            this.toolBarButton4 = new System.Windows.Forms.ToolBarButton();
            this.toolBarButton5 = new System.Windows.Forms.ToolBarButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.tableLayoutPanel1.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // testControl1
            // 
            this.testControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.testControl1.Location = new System.Drawing.Point(3, 3);
            this.testControl1.Name = "testControl1";
            this.testControl1.Size = new System.Drawing.Size(1186, 700);
            this.testControl1.TabIndex = 1;
            // 
            // toolBar1
            // 
            this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.toolBarButtonStart,
            this.toolBarButtonTest,
            this.toolBarButton1,
            this.toolBarButton2,
            this.toolBarButton3,
            this.toolBarButton4,
            this.toolBarButton5});
            this.toolBar1.ButtonSize = new System.Drawing.Size(12, 12);
            this.toolBar1.DropDownArrows = true;
            this.toolBar1.Location = new System.Drawing.Point(0, 0);
            this.toolBar1.Name = "toolBar1";
            this.toolBar1.ShowToolTips = true;
            this.toolBar1.Size = new System.Drawing.Size(1192, 42);
            this.toolBar1.TabIndex = 2;
            this.toolBar1.Visible = false;
            this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
            // 
            // toolBarButtonStart
            // 
            this.toolBarButtonStart.Name = "toolBarButtonStart";
            this.toolBarButtonStart.Text = "start";
            // 
            // toolBarButtonTest
            // 
            this.toolBarButtonTest.Name = "toolBarButtonTest";
            this.toolBarButtonTest.Text = "rethink";
            // 
            // toolBarButton1
            // 
            this.toolBarButton1.Name = "toolBarButton1";
            this.toolBarButton1.Text = "wipe";
            // 
            // toolBarButton2
            // 
            this.toolBarButton2.Name = "toolBarButton2";
            this.toolBarButton2.Text = "rewire";
            // 
            // toolBarButton3
            // 
            this.toolBarButton3.Name = "toolBarButton3";
            this.toolBarButton3.Text = "shrink";
            // 
            // toolBarButton4
            // 
            this.toolBarButton4.Name = "toolBarButton4";
            this.toolBarButton4.Text = "reset";
            // 
            // toolBarButton5
            // 
            this.toolBarButton5.Name = "toolBarButton5";
            this.toolBarButton5.Text = "switch";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.testControl1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 706F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 706F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1192, 706);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.tableLayoutPanel1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1192, 706);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 42);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(1192, 731);
            this.toolStripContainer1.TabIndex = 4;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1192, 773);
            this.Controls.Add(this.toolStripContainer1);
            this.Controls.Add(this.toolBar1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "BuzzNet";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
        {
            e.Button.Enabled = false;

            switch (e.Button.Text)
            {
                case "start":
                    testControl1.ReportEvent("Tickle Brain", "starting", DateTime.Now.ToString());
                    testControl1.TickleBrain();
                    testControl1.ReportEvent("Tickle Brain", "completed", DateTime.Now.ToString());
                    break;
                case "rethink":
                    testControl1.ReportEvent("Brain Rethink", "starting", DateTime.Now.ToString());
                    testControl1.BrainRethink();
                    testControl1.ReportEvent("Brain Rethink", "completed", DateTime.Now.ToString());
                    break;
                case "wipe":
                    testControl1.ReportEvent("Brain Wipeout", "starting", DateTime.Now.ToString());
                    testControl1.WipeBrain();
                    testControl1.ReportEvent("Brain Wipeout", "completed", DateTime.Now.ToString());
                    break;
                case "rewire":
                    testControl1.ReportEvent("Brain Rewire", "starting", DateTime.Now.ToString());
                    testControl1.RewireBrain();
                    testControl1.ReportEvent("Brain Rewire", "completed", DateTime.Now.ToString());
                    break;
                case "shrink":
                    testControl1.ReportEvent("Brain Shrink", "starting", DateTime.Now.ToString());
                    testControl1.ShrinkBrain();
                    testControl1.ReportEvent("Brain Shrink", "completed", DateTime.Now.ToString());
                    break;
                case "reset":
                    testControl1.ReportEvent("Brain Reset", "starting", DateTime.Now.ToString());
                    testControl1.ResetBrain();
                    testControl1.ReportEvent("Brain Reset", "completed", DateTime.Now.ToString());
                    break;
                case "switch":
                    testControl1.ReportEvent("Brain Switch", "starting", DateTime.Now.ToString());
                    testControl1.SwitchBrain();
                    testControl1.ReportEvent("Brain Switch", "completed", DateTime.Now.ToString());
                    break;
                default:
                    break;
            }

            e.Button.Enabled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // doesnt really work as intended. the closing call is still blocking until the saving routine has completed
            // but this is ok for now.
            testControl1.BeforeTerminate();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            testControl1.Terminate();
        }
    }
}
