using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pizza
{
    public partial class PizzaForm : Form
    {
        private string urlPizza = Properties.Settings.Default.UrlPizza;
        private string status = "";
        private DateTime startTime = DateTime.Now;

        public PizzaForm()
        {
            InitializeComponent();
            timerRefresh.Start();
            pizzaCanvas1.Pizza.PizzaEvents += Pizza_PizzaEvents;
        }

        private void Pizza_PizzaEvents(object sender, PizzEventArgs e)
        {
            switch (e.PizzaEvent)
            {
                case PizzaEvent.Loaded:
                case PizzaEvent.Validated:
                case PizzaEvent.Done:
                    MessageBox.Show("Task Complete", "Note", MessageBoxButtons.OK);
                    break;
            }
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            int seed = 0;
            int.TryParse(tbSeed.Text, out seed);
            pizzaCanvas1.Seed = seed;

            PizzaPie.EPickStrategy pickOrder = PizzaPie.EPickStrategy.First;
            Enum.TryParse<PizzaPie.EPickStrategy>(comboBoxPickOrder.Text, out pickOrder);
            pizzaCanvas1.PickOrder = pickOrder;

            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Properties.Settings.Default.UrlDataDirectory;
                openFileDialog.Filter = "in files (*.in)|*.in";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    Properties.Settings.Default.UrlPizza = filePath;
                    Properties.Settings.Default.Save();
                    var t = new Task(() => { this.pizzaCanvas1.LoadPizza(filePath); });
                    t.Start();
                }
            }
        }

        private void buttonSlice_Click(object sender, EventArgs e)
        {
            var t = new Task(() => { this.pizzaCanvas1.SlicePizza(); });
            t.Start();
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            BeginInvoke(new MethodInvoker(RefreshStatus));
        }

        private void RefreshStatus()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(RefreshStatus));
                return;
            }
            status = pizzaCanvas1.GetStatus();
            lblStatus.Text = status;
            lblStatus.Invalidate();

            toolStripStatusLabel.Text = string.Format("Loading time: {0} / Slicing time: {1}", pizzaCanvas1.LoadingTime, pizzaCanvas1.SlicingTime);
            toolStripStatusLabel.Invalidate();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = Properties.Settings.Default.UrlDataDirectory;
                saveFileDialog.Filter = "out files (*.out)|*.out";
                saveFileDialog.FilterIndex = 2;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = saveFileDialog.FileName;
                    Properties.Settings.Default.UrlPizza = filePath;
                    Properties.Settings.Default.Save();
                    new Task(() => { this.pizzaCanvas1.SavePizza(filePath); }).Start();
                }
            }
        }

        private void btnValidate_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Properties.Settings.Default.UrlDataDirectory;
                openFileDialog.Filter = "out files (*.out)|*.out";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    Properties.Settings.Default.UrlPizza = filePath;
                    Properties.Settings.Default.Save();
                    new Task(() => { this.pizzaCanvas1.ValidatePizza(filePath); }).Start();
                }
            }
        }

        private void btnExportPizza_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = Properties.Settings.Default.UrlDataDirectory;
                saveFileDialog.Filter = "png files (*.bmp)|*.bmp";
                saveFileDialog.FilterIndex = 2;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = saveFileDialog.FileName;
                    Properties.Settings.Default.UrlPizzaImg = filePath;
                    Properties.Settings.Default.Save();
                    var t = new Task(() => { this.pizzaCanvas1.Pizza.ExportPizzaAsImg(filePath); });
                    t.Start();

                }
            }
        }

        private void btnExportSlices_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = Properties.Settings.Default.UrlDataDirectory;
                saveFileDialog.Filter = "save files (*.bmp)|*.bmp";
                saveFileDialog.FilterIndex = 2;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = saveFileDialog.FileName;
                    Properties.Settings.Default.UrlSlicedPizzaImg = filePath;
                    Properties.Settings.Default.Save();
                    var t = new Task(() => { this.pizzaCanvas1.Pizza.ExportSlizedPizzaAsImg(filePath); });
                    t.Start();
                }
            }
        }
    }
}
