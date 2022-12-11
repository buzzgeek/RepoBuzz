namespace Pizza
{
    partial class PizzaForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pizzaCanvas1 = new Pizza.PizzaCanvas();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.buttonSlice = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnValidate = new System.Windows.Forms.Button();
            this.tbSeed = new System.Windows.Forms.TextBox();
            this.comboBoxPickOrder = new System.Windows.Forms.ComboBox();
            this.btnExportPizza = new System.Windows.Forms.Button();
            this.btnExportSlizes = new System.Windows.Forms.Button();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.timerRefresh = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.pizzaCanvas1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1037, 507);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // pizzaCanvas1
            // 
            this.pizzaCanvas1.AutoScroll = true;
            this.pizzaCanvas1.AutoScrollMinSize = new System.Drawing.Size(20000, 20000);
            this.pizzaCanvas1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pizzaCanvas1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pizzaCanvas1.LoadingTime = "";
            this.pizzaCanvas1.Location = new System.Drawing.Point(3, 43);
            this.pizzaCanvas1.Name = "pizzaCanvas1";
            this.pizzaCanvas1.PickOrder = Pizza.EPickStrategy.First;
            this.pizzaCanvas1.Seed = 0;
            this.pizzaCanvas1.Size = new System.Drawing.Size(1031, 465);
            this.pizzaCanvas1.SlicingTime = "";
            this.pizzaCanvas1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 9;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel2.Controls.Add(this.buttonLoad, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonSlice, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonSave, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblStatus, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnValidate, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.tbSeed, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.comboBoxPickOrder, 6, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnExportPizza, 7, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnExportSlizes, 8, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1031, 34);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // buttonLoad
            // 
            this.buttonLoad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonLoad.Location = new System.Drawing.Point(414, 3);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(54, 28);
            this.buttonLoad.TabIndex = 0;
            this.buttonLoad.Text = "Load";
            this.buttonLoad.UseVisualStyleBackColor = true;
            this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            // 
            // buttonSlice
            // 
            this.buttonSlice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonSlice.Location = new System.Drawing.Point(474, 3);
            this.buttonSlice.Name = "buttonSlice";
            this.buttonSlice.Size = new System.Drawing.Size(54, 28);
            this.buttonSlice.TabIndex = 1;
            this.buttonSlice.Text = "Slice";
            this.buttonSlice.UseVisualStyleBackColor = true;
            this.buttonSlice.Click += new System.EventHandler(this.buttonSlice_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonSave.Location = new System.Drawing.Point(534, 3);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(54, 28);
            this.buttonSave.TabIndex = 2;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(6, 6);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(6);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnValidate
            // 
            this.btnValidate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnValidate.Location = new System.Drawing.Point(594, 3);
            this.btnValidate.Name = "btnValidate";
            this.btnValidate.Size = new System.Drawing.Size(54, 28);
            this.btnValidate.TabIndex = 4;
            this.btnValidate.Text = "Validate";
            this.btnValidate.UseVisualStyleBackColor = true;
            this.btnValidate.Click += new System.EventHandler(this.btnValidate_Click);
            // 
            // tbSeed
            // 
            this.tbSeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbSeed.Location = new System.Drawing.Point(654, 3);
            this.tbSeed.Name = "tbSeed";
            this.tbSeed.Size = new System.Drawing.Size(54, 20);
            this.tbSeed.TabIndex = 5;
            this.tbSeed.Text = "0";
            // 
            // comboBoxPickOrder
            // 
            this.comboBoxPickOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxPickOrder.FormattingEnabled = true;
            this.comboBoxPickOrder.Items.AddRange(new object[] {
            "First",
            "Last",
            "Random"});
            this.comboBoxPickOrder.Location = new System.Drawing.Point(714, 3);
            this.comboBoxPickOrder.Name = "comboBoxPickOrder";
            this.comboBoxPickOrder.Size = new System.Drawing.Size(74, 21);
            this.comboBoxPickOrder.TabIndex = 6;
            this.comboBoxPickOrder.Text = "First";
            // 
            // btnExportPizza
            // 
            this.btnExportPizza.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnExportPizza.Location = new System.Drawing.Point(794, 3);
            this.btnExportPizza.Name = "btnExportPizza";
            this.btnExportPizza.Size = new System.Drawing.Size(114, 28);
            this.btnExportPizza.TabIndex = 7;
            this.btnExportPizza.Text = "Export Pizza";
            this.btnExportPizza.UseVisualStyleBackColor = true;
            this.btnExportPizza.Click += new System.EventHandler(this.btnExportPizza_Click);
            // 
            // btnExportSlizes
            // 
            this.btnExportSlizes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnExportSlizes.Location = new System.Drawing.Point(914, 3);
            this.btnExportSlizes.Name = "btnExportSlizes";
            this.btnExportSlizes.Size = new System.Drawing.Size(114, 28);
            this.btnExportSlizes.TabIndex = 8;
            this.btnExportSlizes.Text = "Export Slices";
            this.btnExportSlizes.UseVisualStyleBackColor = true;
            this.btnExportSlizes.Click += new System.EventHandler(this.btnExportSlices_Click);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip1);
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.tableLayoutPanel1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1037, 507);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(1037, 554);
            this.toolStripContainer1.TabIndex = 1;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 0);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1037, 22);
            this.statusStrip1.TabIndex = 0;
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.AutoSize = false;
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(1022, 17);
            this.toolStripStatusLabel.Spring = true;
            // 
            // timerRefresh
            // 
            this.timerRefresh.Tick += new System.EventHandler(this.timerRefresh_Tick);
            // 
            // PizzaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1037, 554);
            this.Controls.Add(this.toolStripContainer1);
            this.Name = "PizzaForm";
            this.Text = "Pizza Challenge - BuzzNet";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private PizzaCanvas pizzaCanvas1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button buttonLoad;
        private System.Windows.Forms.Button buttonSlice;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Timer timerRefresh;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.Button btnValidate;
        private System.Windows.Forms.TextBox tbSeed;
        private System.Windows.Forms.ComboBox comboBoxPickOrder;
        private System.Windows.Forms.Button btnExportPizza;
        private System.Windows.Forms.Button btnExportSlizes;
    }
}

