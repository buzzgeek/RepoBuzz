using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Pizza
{
    public partial class PizzaCanvas : UserControl
    {
        private Brush mushroomBrush = Brushes.Teal;
        private Brush tomatoBrush = Brushes.Tan;
        private Brush unknownBrush = Brushes.Black;
        private Brush somethingBrush = Brushes.LightBlue;

        private PizzaPie pizza = new PizzaPie();
        private string urlPizza = Properties.Settings.Default.UrlPizza;
        private string urlResult = Properties.Settings.Default.UrlResult;
        private int tileSize = 20;
        private bool isLoaded = false;
        private bool isValidated = false;

        public int Seed { get; set; } = 0;
        public PizzaPie.EPickStrategy PickOrder { get; set; } = PizzaPie.EPickStrategy.First;

        public string GetStatus()
        {
            return string.Format("Min.Ingredients:{0} / Max.Tiles:{1} / Loaded:{2} / Shapes:{3} / Score:{4} / Missed:{5}", pizza.MinimumTilesPerIngredient, pizza.MaximumTilesPerSlice, pizza.Available, pizza.Slices.Count, pizza.Score, pizza.Missed);
        }

        public PizzaPie Pizza { get { return pizza; } }

        public string LoadingTime { get; set; } = "";
        public string SlicingTime { get; set; } = "";

        public PizzaCanvas()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            base.VerticalScroll.Visible = true;
            base.HorizontalScroll.Visible = true;
            ResizeRedraw = true;
            MouseWheel += PizzaCanvas_MouseWheel;
            AutoScrollMinSize = new Size(20000, 20000);
            HorizontalScroll.Minimum = 0;
            VerticalScroll.Minimum = 0;
            HorizontalScroll.Maximum = 20000;
            VerticalScroll.Maximum = 20000;
            pizza.PizzaEvents += OnPizzaEvent;

        }

        private void OnPizzaEvent(object sender, PizzEventArgs e)
        {
            switch (e.PizzaEvent)
            {
                case PizzaEvent.Loaded:
                    isLoaded = true;
                    Invalidate();
                    break;
                case PizzaEvent.Sliced:
                    Invalidate();
                    break;
                case PizzaEvent.Validated:
                    isValidated = true;
                    Invalidate();
                    break;
            }
        }

        public void LoadPizza(string urlPizza)
        {
            isLoaded = false;
            isValidated = false;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            pizza.Seed = this.Seed;
            pizza.PickOrder = this.PickOrder;
            pizza.Load(urlPizza);
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            LoadingTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Invalidate();
        }

        public void ValidatePizza(string urlResult)
        {
            isLoaded = false;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            pizza.ValidateResult(urlResult);
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            LoadingTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Invalidate();
        }

        public void SavePizza(string urlPizza)
        {
            isLoaded = false;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            pizza.SaveResult(urlPizza);
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            LoadingTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Invalidate();
        }


        public void SlicePizza()
        {
            if (!isLoaded) return;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            pizza.Slice();
            TimeSpan ts = sw.Elapsed;
            SlicingTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Invalidate();
        }

        private void PizzaCanvas_MouseWheel(object sender, MouseEventArgs e)
        {
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int rowOffset = pizza.Rows * VerticalScroll.Value / VerticalScroll.Maximum;
            int colOffset = pizza.Columns * HorizontalScroll.Value / HorizontalScroll.Maximum;
            int rowMax = (Height / tileSize);
            int colMax = (Width / tileSize);

            Brush brush = Brushes.Wheat;
            if (!isLoaded && !isValidated)
            {
                e.Graphics.FillRectangle(brush, new Rectangle(0, 0, Width, Height));
                return;
            }

            for (int r = 0; r < rowMax; r++)
            {
                for (int c = 0; c < colMax; c++)
                {
                    brush = Brushes.Wheat;

                    if ( (isLoaded || isValidated) && 
                        (r + rowOffset < pizza.Rows) &&
                        (c + colOffset < pizza.Columns))
                    { 
                        int relativeR = (r + rowOffset) < pizza.Rows ? (r + rowOffset) : pizza.Rows - 1;
                        int relativeC = (c + colOffset) < pizza.Columns ? (c + colOffset) : pizza.Columns - 1;
                        if (!pizza.Tiles[relativeR][relativeC].Allocated)
                        {
                            switch (pizza.Tiles[relativeR][relativeC].Topping)
                            {
                                case Ingredient.Mushroom:
                                    brush = mushroomBrush;
                                    break;
                                case Ingredient.Tomato:
                                    brush = tomatoBrush;
                                    break;
                                case Ingredient.Unknown:
                                    brush = unknownBrush;
                                    break;
                                case Ingredient.Something:
                                    brush = somethingBrush;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    int x = c * tileSize;
                    int y = r * tileSize;
                    e.Graphics.FillRectangle(brush, new Rectangle(x, y, tileSize, tileSize));
                }
            }
        }

        private void PizzaCanvas_Scroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }
    }
}
