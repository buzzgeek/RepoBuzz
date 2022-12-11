using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace Pizza
{
    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            Array.Clear(Bits, 0, Bits.Length);
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void SetPixel(int x, int y, Color colour)
        {
            int index = x + (y * Width);
            int col = colour.ToArgb();

            if (index >= Bits.Length)
                return;

            Bits[index] = col;
        }

        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Bits[index];
            Color result = Color.FromArgb(col);

            return result;
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }

    public enum eDirection
    {
        Up = 0,
        Right,
        Down,
        Left
    }

    public class Tile
    {
        private int row = 0;
        private int column = 0;
        private Ingredient ingredient = Ingredient.Unknown;
        private HashSet<Slice> possibleSlices = new HashSet<Slice>();
        private bool allocated = false;
        private bool processed = false;
        private int sliceId = -1;

        public Tile(int row, int column, Ingredient ingredient, int sliceId)
        {
            this.row = row;
            this.column = column;
            this.ingredient = ingredient;
            this.sliceId = sliceId;
        }
        public override string ToString()
        {
            return string.Format("r:{0} c:{1}", row, column);
        }

        public Ingredient Topping { get { return ingredient; } set { ingredient = value; } }
        public int Row { get { return row; } }
        public int Column { get { return column; } }
        public bool Allocated
        {
            get { return allocated; }
            set { allocated = value; }
        }

        public bool Processed
        {
            get { return processed; }
            set { processed = value; }
        }

        public HashSet<Slice> PossibleSlices { get { return possibleSlices; } }

        public int SliceId
        {
            get { return sliceId; }
            set { sliceId = value; }
        }
    }

    public enum Ingredient
    {
        Unknown = 0,
        Tomato,
        Mushroom,
        Something,
    }

    public class Slice
    {
        private int id = -1;
        private Tile origin = null;
        private int rows = 0;
        private int columns = 0;
        private HashSet<Tile> tiles = new HashSet<Tile>();

        public int Id { get { return id; } }
        public Tile Origin { get { return origin; } }
        public int Rows { get { return rows; } }
        public int Columns { get { return columns; } }
        public HashSet<Tile> Tiles { get { return tiles; } }

        public Slice(int id, Tile origin, int rows, int columns)
        {
            this.id = id;
            this.origin = origin;
            this.rows = rows;
            this.columns = columns;
        }

        public Slice(int id, Tile origin, Slice slice)
        {
            this.id = id;
            this.origin = origin;
            this.rows = slice.rows;
            this.columns = slice.columns;
        }

        public Slice(int id, int rows, int columns)
        {
            this.id = id;
            this.origin = null;
            this.rows = rows;
            this.columns = columns;
        }

        public override string ToString()
        {
            return string.Format("origin:{0} r:{1} c:{2}", origin != null ? origin.ToString() : "", rows, columns);
        }
    }

    public interface IPizza
    {
        int Seed { get; set; }
        int Score { get; }
        int MinimumTilesPerIngredient { get; }
        int MaximumTilesPerSlice { get; }
        int Rows { get; }
        int Columns { get; }
        IList<Slice> Slices { get; }
        IList<List<Tile>> Tiles { get; }
        int Missed { get; }
        int Available { get; }
        EPickStrategy PickOrder { get; set; }

        void Load(string urlPizza);
        void ExportPizzaAsImg(string urlPizzaImg);
        void ExportSlizedPizzaAsImg(string urlPizzaImg);
        void SaveResult(string urlResult);
        void Slice();
        void ValidateResult(string urlResult);

        event PizzaEventHandler PizzaEvents;
    }

    public enum EPickStrategy
    {
        First,
        Last,
        Random,
    }

    public enum PizzaEvent
    {
        Loaded = 0,
        Loading,
        Slicing,
        Sliced,
        Done,
        Validated,
    }

    public class PizzEventArgs : EventArgs
    {
        public Slice Slice { get; set; }
        public PizzaEvent PizzaEvent { get; set; }
        public PizzEventArgs(PizzaEvent pizzaEvent, Slice slice)
        {
            this.PizzaEvent = pizzaEvent;
            this.Slice = slice;
        }
    }

    public delegate void PizzaEventHandler(Object sender, PizzEventArgs e);

    public class PizzaPie : IPizza
    {
        private Color[] sliceColors = new Color[] { Color.Blue,
            Color.Red,
            Color.Yellow,
            Color.Green,
            Color.Orange,
            Color.Turquoise,
            Color.Indigo,
            Color.HotPink,
            Color.Purple };

        private int rows = 0;
        private int minimumTilesPerIngredient = 0;
        private int maximumTilesPerSlice = 0;
        private int columns = 0;
        private List<List<Tile>> tiles = new List<List<Tile>>();
        private List<Tile> allTiles = new List<Tile>();
        private SortedDictionary<string, Tile> sortedTiles = new SortedDictionary<string, Tile>();
        private List<Slice> slices = new List<Slice>();
        private Dictionary<string, Slice> possibleShapes = new Dictionary<string, Slice>();
        private int missedTiles = 0;
        private int loadedTiles = 0;
        private int allocatedTiles = 0;
        private int progress = 0;
        Random rnd = null;

        public int MinimumTilesPerIngredient { get { return minimumTilesPerIngredient; } }
        public int MaximumTilesPerSlice { get { return maximumTilesPerSlice; } }
        public int Rows { get { return rows; } }
        public int Columns { get { return columns; } }
        public IList<Slice> Slices { get { return slices; } }
        public IList<List<Tile>> Tiles { get { return tiles; } }
        public int Score { get => allocatedTiles; }
        public int Missed { get => missedTiles; }
        public int Available { get => LoadedTiles; }
        public int LoadedTiles { get => loadedTiles; }
        public int Progress { get => progress; }

        public event PizzaEventHandler PizzaEvents;

        public int Seed { get; set; } = 0;
        public EPickStrategy PickOrder { get; set; } = EPickStrategy.First;

        protected virtual void RaisePizzaEvent(PizzEventArgs e)
        {
            PizzaEvents?.Invoke(this, e);
        }

        public virtual void Load(string urlPizza)
        {
            rnd = new Random(Seed);
            rows = 0;
            minimumTilesPerIngredient = 0;
            maximumTilesPerSlice = 0;
            columns = 0;
            tiles.Clear();
            sortedTiles.Clear();
            slices.Clear();
            possibleShapes.Clear();
            progress = 0;
            loadedTiles = 0;
            missedTiles = 0;
            allocatedTiles = 0;
            allTiles.Clear();
            LoadTiles(urlPizza);
            CreateListOfValidShapes(true, true);
            LoadExtra();

            RaisePizzaEvent(new PizzEventArgs(PizzaEvent.Loaded, null));
        }

        protected virtual void LoadExtra()
        {
            return;
        }

        public void ValidateResult(string urlResult)
        {
            if (!File.Exists(urlResult))
            {
                return;
            }

            tiles.Clear();
            slices.Clear();
            for (int r = 0; r < rows; r++)
            {
                tiles.Add(new List<Tile>());
                for (int c = 0; c < columns; c++)
                {
                    Tile tile = new Tile(r, c, Ingredient.Unknown, -1);
                    tiles[r].Add(tile);
                }
            }

            string[] lines = File.ReadAllLines(urlResult);

            int numSlices = int.Parse(lines[0]);
            int sliceId = 0;

            for (int i = 1; i < lines.Length; i++)
            {
                string[] s = lines[i].Split(' ');
                Tile origin = new Tile(int.Parse(s[0]), int.Parse(s[1]), Ingredient.Something, -1);
                Slice slice = new Slice(sliceId, origin, int.Parse(s[2]) - origin.Row, int.Parse(s[3]) - origin.Column);
                slices.Add(slice);
                for (int r = 0; r <= slice.Rows; r++)
                {
                    for (int c = 0; c <= slice.Columns; c++)
                    {
                        // check for overlapping slices
                        Debug.Assert(tiles[slice.Origin.Row + r][slice.Origin.Column + c].Topping == Ingredient.Unknown);

                        tiles[slice.Origin.Row + r][slice.Origin.Column + c].Allocated = false;
                        tiles[slice.Origin.Row + r][slice.Origin.Column + c].Topping = Ingredient.Something;
                        tiles[slice.Origin.Row + r][slice.Origin.Column + c].SliceId = -1;
                    }
                    sliceId++;
                }
            }

            Debug.Assert(numSlices == slices.Count);
            RaisePizzaEvent(new PizzEventArgs(PizzaEvent.Validated, null));
        }


        private void CutDistinctSlices()
        {
            // remove all tiles that can only be part of a single slice.
            // surprisingly, this does not lead to a good result

            bool canCut = false;
            do
            {
                canCut = false;
                foreach (Tile tile in allTiles)
                {
                    tile.PossibleSlices.Clear();
                }

                int sliceId = 0;
                foreach (Tile tile in allTiles)
                {
                    foreach (Slice s in possibleShapes.Values)
                    {
                        if (IsValidShape(s, tile))
                        {
                            Slice slice = new Slice(sliceId++, tile, s);
                            for (int r = 0; r < s.Rows; r++)
                            {
                                for (int c = 0; c < s.Columns; c++)
                                {
                                    tiles[tile.Row + r][tile.Column + c].PossibleSlices.Add(slice);
                                }
                            }
                        }
                    }
                }

                var check = from tile in allTiles where tile.PossibleSlices.Count == 1 select tile;
                if (check.Any())
                {
                    canCut = true;
                    Console.WriteLine("huray, found some tiles that have only one possible slice");
                    foreach (Tile t in check)
                    {
                        Slice s = t.PossibleSlices.First<Slice>();
                        CutPizza(s);
                    }
                }
            } while (canCut);

            // clean up the remaining possible shapes to regain some memory
            foreach (Tile tile in allTiles)
            {
                tile.PossibleSlices.Clear();
            }

        }

        private void CreateListOfValidShapes(bool generateLines, bool generateSquares)
        {
            int sliceId = 0;
            // lines
            if (generateLines)
            {
                for (int x = 2 * minimumTilesPerIngredient; x <= maximumTilesPerSlice; x++)
                {
                    for (int y = 1; y < x; y++)
                    {
                        if (x % y == 0)
                        {
                            int r = y;
                            int c = x / y;
                            string key = string.Format("{0} {1}", r, c);

                            if (r != 1 && c != 1) continue;

                            if (!possibleShapes.ContainsKey(key))
                            {
                                possibleShapes[key] = new Slice(sliceId++, r, c);
                            }
                            if (r != c)
                            {
                                key = string.Format("{0} {1}", c, r);
                                if (!possibleShapes.ContainsKey(key))
                                {
                                    possibleShapes[key] = new Slice(sliceId++, c, r);
                                }
                            }
                        }
                    }
                }
            }
            if (generateSquares)
            {
                //squares
                for (int x = 2 * minimumTilesPerIngredient; x <= maximumTilesPerSlice; x++)
                {
                    for (int y = 1; y < x; y++)
                    {
                        if (x % y == 0)
                        {
                            int r = y;
                            int c = x / y;
                            string key = string.Format("{0} {1}", r, c);

                            if (r == 1 || c == 1) continue;

                            if (!possibleShapes.ContainsKey(key))
                            {
                                possibleShapes[key] = new Slice(sliceId++, r, c);
                            }
                            if (r != c)
                            {
                                key = string.Format("{0} {1}", c, r);
                                if (!possibleShapes.ContainsKey(key))
                                {
                                    possibleShapes[key] = new Slice(sliceId++, c, r);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LoadTiles(string urlPizza)
        {
            try
            {
                if (!File.Exists(urlPizza))
                {
                    return;
                }

                string[] lines = File.ReadAllLines(urlPizza);
                string[] specs = lines[0].Split(' ');
                rows = int.Parse(specs[0]);
                columns = int.Parse(specs[1]);
                minimumTilesPerIngredient = int.Parse(specs[2]);
                maximumTilesPerSlice = int.Parse(specs[3]);

                int rowIndex = 0;
                for (int i = 1; i < lines.Length; i++)
                {
                    int colIndex = 0;
                    tiles.Add(new List<Tile>());
                    foreach (char ingredient in lines[i])
                    {
                        Tile tile = null;
                        switch (ingredient)
                        {
                            case 'T':
                                tile = new Tile(rowIndex, colIndex, Ingredient.Tomato, -1);
                                break;
                            case 'M':
                                tile = new Tile(rowIndex, colIndex, Ingredient.Mushroom, -1);
                                break;
                            default:
                                // nothing to do for now
                                break;
                        }
                        if (tile != null)
                        {
                            tiles[rowIndex].Add(tile);
                            allTiles.Add(tile);
                            string key = string.Format("{0:0000} {1:0000}", tile.Row, tile.Column);
                            sortedTiles[key] = tile;
                        }
                        colIndex++;
                        loadedTiles++;
                        progress = (LoadedTiles * 100) / (Rows * Columns);
                        RaisePizzaEvent(new PizzEventArgs(PizzaEvent.Slicing, null));
                    }
                    rowIndex++;
                    RaisePizzaEvent(new PizzEventArgs(PizzaEvent.Loading, null));
                }
                progress = 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        public void SaveResult(string urlResult)
        {
            try
            {
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(urlResult))
                {
                    file.WriteLine(string.Format("{0}", slices.Count));
                    foreach (Slice s in slices)
                    {
                        file.WriteLine(string.Format("{0} {1} {2} {3}", s.Origin.Row, s.Origin.Column, s.Origin.Row + s.Rows - 1, s.Origin.Column + s.Columns - 1));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        protected virtual void CommenceSlicing()
        {
            try
            {
                if (Properties.Settings.Default.CutDistinctSlices)
                {
                    // this one does not perform well, I expected a different result
                    CutDistinctSlices();
                }

                bool isDone = false;

                progress = 0;
                int sliceId = 0;
                while (!isDone)
                {
                    Tile tile = LocateAvailableTile(PickOrder);
                    if (tile == null)
                    {
                        isDone = true;
                        break;
                    }

                    Slice slice = GetValidSlice(tile, Properties.Settings.Default.CheckNeighbour);

                    if (slice != null)
                    {
                        Slice s = new Slice(sliceId++, tile, slice);
                        CutPizza(s);
                        RaisePizzaEvent(new PizzEventArgs(PizzaEvent.Sliced, slice));
                    }
                    else
                    {
                        tile.Processed = true;
                        string key = string.Format("{0:0000} {1:0000}", tile.Row, tile.Column);
                        sortedTiles.Remove(key);
                    }
                    missedTiles = (Rows * Columns) - allocatedTiles;

                    progress = sortedTiles.Count * 100 / (Rows * Columns);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        public void Slice()
        {
            var t = new Task(() => { CommenceSlicing(); });
            t.Start();
            t.Wait();

            RaisePizzaEvent(new PizzEventArgs(PizzaEvent.Done, null));
        }

        private Tile LocateAvailableTile(EPickStrategy strategy)
        {
            if (!sortedTiles.Any())
                return null;

            switch (strategy)
            {
                case EPickStrategy.First:
                    return sortedTiles.First().Value;
                case EPickStrategy.Last:
                    return sortedTiles.Last().Value;
                case EPickStrategy.Random:
                    return sortedTiles.ElementAt(rnd.Next(sortedTiles.Count)).Value;
                default:
                    throw new NotImplementedException();
            }
        }


        private void CutPizza(Slice slice)
        {
            for (int r = 0; r < slice.Rows; r++)
            {
                for (int c = 0; c < slice.Columns; c++)
                {
                    tiles[slice.Origin.Row + r][c + slice.Origin.Column].Allocated = true;
                    tiles[slice.Origin.Row + r][c + slice.Origin.Column].Processed = true;
                    tiles[slice.Origin.Row + r][slice.Origin.Column + c].SliceId = slice.Id;

                    string key = string.Format("{0:0000} {1:0000}", slice.Origin.Row + r, c + slice.Origin.Column);
                    if (sortedTiles.ContainsKey(key)) sortedTiles.Remove(key);
                }
            }
            allocatedTiles += (slice.Rows * slice.Columns);
            Slices.Add(slice);
        }

        // TODO: need to find a better strategy to find the best slice
        private Slice GetValidSlice(Tile tile, bool checkNeighbours = false)
        {
            Slice result = null;

            IEnumerable<Slice> posSlices = Properties.Settings.Default.ReversePossibleShapes ? possibleShapes.Values.Reverse() : possibleShapes.Values;

            foreach (var shape in posSlices)
            {
                if (IsValidShape(shape, tile))
                {
                    if (checkNeighbours)
                    {
                        bool isValid = (tile.Column == Columns) || (tile.Row == Rows);
                        if (!isValid)
                        {
                            int row = (tile.Row + shape.Rows - 1);
                            int col = (tile.Column + shape.Columns - 1);
                            isValid = (GetValidSlice(tiles[row][tile.Column], false) != null)
                                || (GetValidSlice(tiles[tile.Row][col], false) != null);
                        }

                        if (isValid)
                        {
                            return shape;
                        }
                    }
                    else
                    {
                        return shape;
                    }
                }
            }
            return result;
        }

        private bool IsValidShape(Slice shape, Tile tile)
        {
            // check if the shape is to large
            if ((tile.Row + shape.Rows) > Rows || (tile.Column + shape.Columns) > Columns)
            {
                return false;
            }

            int mushrooms = 0;
            int tomatos = 0;

            for (int r = 0; r < shape.Rows; r++)
            {
                for (int c = 0; c < shape.Columns; c++)
                {
                    if (tiles[tile.Row + r][c + tile.Column].Allocated)
                    {
                        return false;
                    }

                    switch (tiles[tile.Row + r][c + tile.Column].Topping)
                    {
                        case Ingredient.Mushroom:
                            mushrooms++;
                            break;
                        case Ingredient.Tomato:
                            tomatos++;
                            break;
                    }
                }
            }
            return (mushrooms >= minimumTilesPerIngredient && tomatos >= minimumTilesPerIngredient);
        }

        public void ExportPizzaAsImg(string urlPizzaImg)
        {
            try
            {
                using (DirectBitmap pizzaImage = new DirectBitmap(Columns, Rows))
                {

                    foreach (var tileRow in Tiles)
                    {
                        foreach (var tile in tileRow)
                        {
                            switch (tile.Topping)
                            {
                                case Ingredient.Tomato:
                                    pizzaImage.SetPixel(tile.Column, tile.Row, Color.Red);
                                    break;
                                case Ingredient.Mushroom:
                                    pizzaImage.SetPixel(tile.Column, tile.Row, Color.Blue);
                                    break;
                                default:
                                    pizzaImage.SetPixel(tile.Column, tile.Row, Color.Green);
                                    break;
                            }
                        }
                    }

                    pizzaImage.Bitmap.Save(urlPizzaImg, ImageFormat.Bmp);
                    RaisePizzaEvent(new PizzEventArgs(PizzaEvent.Done, null));
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        public void ExportSlizedPizzaAsImg(string urlSlizedPizzaImg)
        {
            try
            {
                using (DirectBitmap pizzaSliceImage = new DirectBitmap(Columns, Rows))
                {
                    foreach (var slice in Slices)
                    {
                        for (int r = 0; r < slice.Rows; r++)
                        {
                            for (int c = 0; c < slice.Columns; c++)
                            {
                                try
                                {
                                    pizzaSliceImage.SetPixel(slice.Origin.Column + c, slice.Origin.Row + r, sliceColors[slice.Id % sliceColors.Length]);
                                }
                                catch (Exception ex)
                                {
                                    Console.Error.WriteLine(ex);
                                }
                            }
                        }
                    }
                    pizzaSliceImage.Bitmap.Save(urlSlizedPizzaImg, ImageFormat.Bmp);
                    RaisePizzaEvent(new PizzEventArgs(PizzaEvent.Done, null));
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}
