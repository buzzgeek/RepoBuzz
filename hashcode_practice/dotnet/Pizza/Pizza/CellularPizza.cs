using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Pizza
{

    public class CellularPizza : PizzaPie, IPizza
    {
        private IList<IList<Cell>> cellMap = new List<IList<Cell>>();
        private IList<Cell> activeCells = new List<Cell>();

        protected override void LoadExtra()
        {
            cellMap.Clear();
            activeCells.Clear();
            GenerateCells();
        }

        private void generate_matrix(int rows, int cols, List<List<int>> matrix)
        {
            matrix.Clear();
            for (int r = 0; r < rows; r++)
            {
                matrix.Add(new List<int>());
                for (int c = 0; c < cols; c++)
                {
                    if (r % 2 == 0)
                    {
                        // Even rows
                        matrix[r].Add(((r * cols) + c + 1) - 1);
                    }
                    else
                    {
                        // Odd rows
                        matrix[r].Add(((r + 1) * cols - c) - 1);
                    }
                }
            }
        }

        private void GenerateCells()
        {
            Dictionary<EOrganismStrategy, List<List<int>>> patternMap = new Dictionary<EOrganismStrategy, List<List<int>>>();

            foreach (EOrganismStrategy value in Enum.GetValues(typeof(EOrganismStrategy)))
            {
                List<List<int>> m = new List<List<int>>();
                switch (value)
                {
                    case EOrganismStrategy._1x14:
                        generate_matrix(1, 14, m);
                        break;
                    case EOrganismStrategy._1x13:
                        generate_matrix(1, 13, m);
                        break;
                    case EOrganismStrategy._1x12:
                        generate_matrix(1, 12, m);
                        break;
                    case EOrganismStrategy._2x7:
                        generate_matrix(2, 7, m);
                        break;
                    case EOrganismStrategy._2x6:
                        generate_matrix(2, 6, m);
                        break;
                    case EOrganismStrategy._3x4:
                        generate_matrix(4, 5, m);
                        break;
                    case EOrganismStrategy._14x1:
                        generate_matrix(14, 1, m);
                        break;
                    case EOrganismStrategy._13x1:
                        generate_matrix(13, 1, m);
                        break;
                    case EOrganismStrategy._12x1:
                        generate_matrix(12, 1, m);
                        break;
                    case EOrganismStrategy._7x2:
                        generate_matrix(7, 2, m);
                        break;
                    case EOrganismStrategy._6x2:
                        generate_matrix(6, 2, m);
                        break;
                    case EOrganismStrategy._4x3:
                        generate_matrix(4, 3, m);
                        break;
                    default:
                        generate_matrix(1, 14, m);
                        break;
                }
                patternMap[value] = m;
            }


            int id = 0;
            for (int r = 0; r < Tiles.Count; r++)
            {
                cellMap.Add(new List<Cell>());
                int c = 0;
                foreach (var tile in Tiles[r])
                {
                    Cell cell = new Cell(this, id++, new Position() { C = c++, R = r }, tile.Topping, cellMap, patternMap);
                    cellMap[r].Add(cell);
                    activeCells.Add(cell);
                }
            }

            Console.WriteLine("");
        }

        protected override void CommenceSlicing()
        {
            // keep slicing as long there are active cells
            IList<Cell> removeCells = new List<Cell>();
            while (activeCells.Count > 0)
            {
                Cell first = activeCells.First();


                Debug.Assert(!(first.Status == ECellStatus.invalid && first.Strategy == EOrganismStrategy.none), "This should not happen anymore...");
                // this while check should really not happen anymore - deprecated
                //while (first.Status == ECellStatus.invalid && first.Strategy == EOrganismStrategy.none)
                //{
                //    // if we are here there is something wrong
                //    activeCells.Remove(first);
                //    first = activeCells.First();
                //}

                if (first.Status == ECellStatus.inactive)
                {
                    first.Reset();
                    first.IndexOrder = 0;
                    first.OrganismId = first.Id;
                }

                bool exitLoop = false;

                foreach (var cell in activeCells)
                {
                    if (cell.Position.C > first.Position.C + 14)
                        continue;
                    if (cell.Position.R > first.Position.R + 14)
                        continue;

                    //Cell cell = cellMap[first.Position.X + x][first.Position.Y + y];
                    cell.LifeCycle();

                    // update the tile map to reflect the "slices"
                    Tiles[cell.Position.R][cell.Position.C].SliceId = cell.OrganismId;
                    Tiles[cell.Position.R][cell.Position.C].Allocated = false;
                    Tiles[cell.Position.R][cell.Position.C].Processed = false;


                    switch (cell.Status)
                    {
                        case ECellStatus.active:
//                            Tiles[cell.Position.R][cell.Position.C].Allocated = true;
                            Tiles[cell.Position.R][cell.Position.C].Processed = true;
                            Tiles[cell.Position.R][cell.Position.C].SliceId = cell.OrganismId;
                            break;
                        case ECellStatus.inactive:
                            break;
                        case ECellStatus.invalid:
                            if (cell.OrganismId == cell.Id)
                                exitLoop = true;
                            break;
                        case ECellStatus.valid:
                            Tiles[cell.Position.R][cell.Position.C].Allocated = true;
                            Tiles[cell.Position.R][cell.Position.C].Processed = true;
                            Tiles[cell.Position.R][cell.Position.C].SliceId = cell.OrganismId;
                            removeCells.Add(cell);
                            if (cell.OrganismId == cell.Id)
                                exitLoop = true;
                            break;
                        case ECellStatus.dead:
                            removeCells.Add(cell);
                            exitLoop = true;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    if (exitLoop)
                        break;
                }
                foreach (var cell in removeCells)
                {
                    activeCells.Remove(cell);
                }
                removeCells.Clear();
                RaisePizzaEvent(new PizzEventArgs(PizzaEvent.Sliced, null));
            }
        }
    }
}
