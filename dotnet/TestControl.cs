using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Diagnostics;

namespace BuzzNet
{
	/// <summary>
	/// Summary description for TestControl.
	/// </summary>
	public class TestControl : System.Windows.Forms.UserControl
	{
		#region constants
		#endregion constants

		#region members

        private int REFRESH_RATE = Properties.Settings.Default.UIRefreshRate;
		private System.Windows.Forms.DataGrid dataGrid1;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private DataTable dt = new DataTable("Events");

		private Brush brush;
		private Pen pen;
		private Rectangle windowRect;
		private Random rand = new Random();
		private Brain brain = null;
        //private Brain brain2 = null;
		private Timer timer = null;
		private Brush backBrush = new SolidBrush(Color.White) as Brush;
        private BackgroundWorker initBrain = new BackgroundWorker();
		private Brain curBrain = null;
		private bool run = false;
        private TableLayoutPanel tableLayoutPanel1;
        private FormDiagnostics diagnostics = new FormDiagnostics();
        //private bool brainIsAvailable = false;

        #endregion members

        #region properties
        #endregion properties

        #region constructor

        public TestControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// initialize data grid
			InitializeDataGrid();

			// initialize graphic resources
			InitializeGraphicResources();

            initBrain.DoWork += OnInitBrain;
            initBrain.RunWorkerCompleted += OnBrainComplete;

            initBrain.RunWorkerAsync();
			timer = new Timer();
            timer.Interval = REFRESH_RATE;
			timer.Tick += new EventHandler(OnTick);
			timer.Start();
            diagnostics.Show(this);
        }

        #endregion constructor

        #region public methods

        public void ReportEvent( string action, string result, string comment)
		{
			DataRow dr = dt.NewRow();
			dr[0] = action;
			dr[1] = result;
			dr[2] = comment;
			dt.Rows.InsertAt(dr, 0);
		}

		public void TickleBrain()
		{
			run = !run;
		}

		public void BrainRethink()
		{
			run = false;
            if(curBrain != null)
			curBrain.Rethink();
			Invalidate();
		}

		public void WipeBrain()
		{
			run = false;
            if (curBrain != null)
                curBrain.Wipeout();
			Invalidate();
		}

		public void RewireBrain()
		{
			run = false;
            if (curBrain != null)
                curBrain.Rewire();
			Invalidate();
		}

		public void ShrinkBrain()
		{
			run = false;
            if (curBrain != null)
                curBrain.Shrink(5, 1);
			Invalidate();
		}

		public void ResetBrain()
		{
			run = false;
            if (curBrain != null)
                curBrain.Reset();
			Invalidate();
		}

		public void SwitchBrain()
		{
			run = false;
            curBrain = brain;
            //curBrain = curBrain.Equals(brain) ? brain2 : brain;
			Invalidate();
		}

        public void BeforeTerminate()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(BeforeTerminate));
                return;
            }
            if (brain != null)
            {
                brain.BeforeTerminate();
            }
        }

        public void Terminate()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(Terminate));
                return;
            }
            if (brain != null)
            {
                brain.Terminate();
            }
        }

        #endregion public methods

        #region protected methods

        protected override void OnPaint(PaintEventArgs e)
		{
            if (curBrain != null)
                BrainGUI.Draw(e.Graphics, curBrain);
        }

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground (e);
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				brain.Shutdown();
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#endregion protected methods

		#region private methods

        private void OnBrainComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            //brainIsAvailable = true;
            TickleBrain();
            //MessageBox.Show("Brain is availble ;)", "Note", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnInitBrain(object sender, DoWorkEventArgs e)
        {
            Brain.Setup();
            BrainGUI.Setup();
            brain = new Brain(this, 42, this.ClientSize.Width);
            //brain.CreateGrayMatter();
            curBrain = brain;
            brain.Activate();
        }

		private void InitializeDataGrid()
		{
			dt.Columns.Add(new DataColumn("Action", typeof(string)));
			dt.Columns.Add(new DataColumn("Result", typeof(string)));
			dt.Columns.Add(new DataColumn("Comment", typeof(string)));
			
			this.dataGrid1.DataSource = dt;
		}

		private void InitializeGraphicResources()
		{
			ClientSize = new Size(Width, Height);
			brush = new SolidBrush(Color.White);
			pen = new Pen(new SolidBrush(Color.Wheat)); 
			windowRect = new Rectangle(0, 0, Width, Height);
			SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
		}

		private void OnTick(object sender, EventArgs e)
		{
            if(run && curBrain != null && curBrain.Active)
                Invalidate();
        }

        #endregion private methods

        #region internal methods

        internal void AddDataPoint(double error, double accuracy, ulong index)
        {
            if (diagnostics != null && diagnostics.Visible)
            {
                diagnostics.AddDataPoint(error, accuracy, index);
            }
        }

        #endregion internal methods

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
		{
            this.dataGrid1 = new System.Windows.Forms.DataGrid();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGrid1
            // 
            this.dataGrid1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dataGrid1.DataMember = "";
            this.dataGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGrid1.Location = new System.Drawing.Point(3, 3);
            this.dataGrid1.Name = "dataGrid1";
            this.dataGrid1.ReadOnly = true;
            this.dataGrid1.Size = new System.Drawing.Size(806, 73);
            this.dataGrid1.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.dataGrid1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 463);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 79.02736F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(812, 79);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // TestControl
            // 
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "TestControl";
            this.Size = new System.Drawing.Size(812, 542);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion
	}
}
