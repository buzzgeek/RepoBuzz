using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pizza
{
    // important rules
    // cells are only able to change there own states, they cannot change states in other cells
    // cells can read their own states as well as states of their neighbouring cells

    // NOTE
    // this is currently used as a proof of concept and will only work for the big data input
    // with the constraint max 14 tiles, with min 6 ingredietns each
    public enum ECellStatus { active = 0, inactive, valid, invalid, dead };

    public enum EOrganismStatus { growing, dying, stable }; // once a single cell dies all others must follow

    // this is very specific to the big data set see comment above, note that the order of the strategies is relevant
    public enum EOrganismStrategy { none = -1, _1x14 = 0, _1x13, _1x12, _2x7, _2x6, _3x4, _14x1, _13x1, _12x1, _7x2, _6x2, _4x3 }

    public class Position
    {
        public int C { get; set; }
        public int R { get; set; }
    }

    interface ICell
    {
        IPizza Pizza { get; }
        int Id { get; set; }
        int OrganismId { get; set; }
        Position Position { get; set; }
        Position Predecessor { get; set; }
        Position Successor { get; set; }
        int IndexOrder { get; set; }   // -1: unassigned, 0: mother cell >0: child cell and it's order
        ECellStatus Status { get; set; }
        Ingredient Content { get; } // the cell content which is either a Tomato or a Mushroom 
        EOrganismStrategy Strategy { get; set; }
        int Score { get; set; } //0 is the desired score but that also depends on the Size --> ++ for every T and -- for every M
        int Size { get; set; } // the number of cells currently assigned to the Organism 
        int RequiredSize { get; } //is used to dermine the exact rquired shape (eg only rectangles)

        List<List<int>> PatternMatrix { get; }
        Dictionary<EOrganismStrategy, List<List<int>>> PatternMap { get; set; }

        EOrganismStatus OrganismStatus { get; }
        IList<IList<Cell>> CellMap { get; } // the world the cell lives in
        void LifeCycle();
    }

    public class Cell : ICell
    {
        static readonly int MAX_SIZE = 14;
        static readonly int MIN_SIZE = 12;
        static readonly int MAX_C = 1000;
        static readonly int MAX_R = 1000;
        static readonly int MIN_INGREDIENT = 6; // not really necessary can be deducted via the score

        private IList<IList<Cell>> cellMap;
        private EOrganismStrategy strategy;
        private IPizza pizza;

        // TODO: no default constructor, still experimenting which constructor is the best implementation for the desired approach

        /// <summary>
        /// The initial cell is inactive and only knows its internal states
        /// </summary>
        /// <param name="content"></param>
        public Cell(IPizza pizza, int id, Position position, Ingredient content, IList<IList<Cell>> cellMap, Dictionary<EOrganismStrategy, List<List<int>>> patternMap)
        {
            this.pizza = pizza;
            Position = new Position() { R = position.R, C = position.C };
            Id = id;
            OrganismId = -1;
            Content = content;
            IndexOrder = -1;
            Status = ECellStatus.inactive;
            Strategy = EOrganismStrategy._1x12;
            this.cellMap = cellMap;
            PatternMap = patternMap;
            Reset();
        }

        public IPizza Pizza { get { return pizza; } }
        //List<List<int>> matrix = new List<List<int>>();
        public Dictionary<EOrganismStrategy, List<List<int>>> PatternMap { get; set; }

        // the cell's  states/properties
        #region internal_states
        public IList<IList<Cell>> CellMap { get { return cellMap; } }
        public int Id { get; set; }
        public Position Position { get; set; }
        public int IndexOrder { get; set; }   // -1: unassigned, 0: mother cell >0: child cell and it's order
        public ECellStatus Status { get; set; }
        // optional property eg. not really required for this algorithm to function, so it can be safely omitted
        public Ingredient Content { get; } // the cell content which is either a Tomato or a Mushroom 

        // useful to optimize calculation time, eg do not recalculate for every cycle,
        // might be a problem though if we dont have enough available memory in the planned shader implementation
        public Position Predecessor { get; set; }
        public Position Successor { get; set; }

        public List<List<int>> PatternMatrix
        {
            get
            {
                return PatternMap[Strategy];
            }
        }

        public int RequiredSize
        {
            get
            {
                return PatternMatrix.Count * PatternMatrix[0].Count;
            }
        }
        #endregion 

        // the organism state properties
        // Transcendent properties need to be propagated to all cells of the organism 
        // this will be achieved later on hopefully by backward propagation to the mother cell
        #region trascendent_states
        public int OrganismId { get; set; }
        public EOrganismStrategy Strategy
        {
            get { return strategy; }
            set { strategy = value; }
        }
        public int Score { get; set; } //0 is the desired score but that also depends on the Size --> ++ for every T and -- for every M
        public int Size { get; set; } // the number of cells currently assigned to the Organism 
        // computational properties eg getters
        public EOrganismStatus OrganismStatus
        {
            get
            {
                if (Size < RequiredSize)
                {
                    return EOrganismStatus.growing;
                }
                else if (Size == RequiredSize)
                {
                    if (Score == 0 && Size == MIN_SIZE)
                        return EOrganismStatus.stable;
                    else if (Size > MIN_SIZE && Math.Abs(Score) <= (Size - MIN_SIZE))
                        return EOrganismStatus.stable;
                    else
                        return EOrganismStatus.dying;
                }
                else if (Size > RequiredSize)
                {
                    // the organsim cannot exist
                    return EOrganismStatus.dying;
                }
                return EOrganismStatus.dying;
            }
        }
        #endregion 

        public void LifeCycle()
        {
            switch (Status)
            {
                case ECellStatus.inactive:
                    if (DeteremineIfMotherCell())
                    {
                        IndexOrder = 0; // 0 - indicated Mother cell
                        OrganismId = Id;
                        Score = Content == Ingredient.Tomato ? 1 : -1;
                        Size = 1;
                        if (Strategy == EOrganismStrategy.none)
                        {
                            Strategy = EOrganismStrategy._1x12; // select the first available strategy
                        }
                        Status = ECellStatus.active; // become active
                    }
                    else
                    {
                        // check if there is an organism in the vicinity that requires this cell to become active
                        // if so find the predecessor cell
                        var predecessor = FindPredecessorOfHostOrganism();
                        if (predecessor != null)
                        { // this cell needs become part of the organism
                            OrganismId = predecessor.OrganismId;
                            IndexOrder = predecessor.IndexOrder + 1;
                            Strategy = predecessor.Strategy;
                            Score = Content == Ingredient.Tomato ? 1 : -1;
                            Score = predecessor.Score + Score;
                            Size = predecessor.Size + 1;
                            // set the status of the cell by checking the state of the overall Organism

                            // check if this cell is not part of an actual organism already
                            bool isValidCandidate = !Pizza.Tiles[Position.R][Position.C].Allocated;

                            Debug.Assert(isValidCandidate, "Should not happen anymore...");

                            Status = DeterminCellStatus(isValidCandidate);
                        }
                    }
                    break;
                case ECellStatus.active:
                    var successor = GetSuccessor();
                    if (successor == null)
                    {
                        // there is no more successor eg. calculate the final Status of the organism
                        if (Status != ECellStatus.invalid)
                            Status = DeterminCellStatus(true);
                    }
                    else if (OrganismId != successor.OrganismId && 
                        Pizza.Tiles[successor.Position.R][successor.Position.C].Allocated)
                    {
                        Status = ECellStatus.invalid;
                    }
                    else
                    {
                        Status = successor.Status;
                        Score = successor.Score;
                        Size = successor.Size;
                    }
                    break;
                case ECellStatus.invalid:
                    // check if this cell needs to become a mother cell
                    if (IndexOrder == 0)
                    { // if mother cell see if strategy needs to be changed
                        // reset the global properties    
                        switch (Strategy)
                        {
                            case EOrganismStrategy._1x12:
                                Strategy = EOrganismStrategy._1x13;
                                break;
                            case EOrganismStrategy._1x13:
                                Strategy = EOrganismStrategy._1x14;
                                break;
                            case EOrganismStrategy._1x14:
                                Strategy = EOrganismStrategy._12x1;
                                break;
                            case EOrganismStrategy._12x1:
                                Strategy = EOrganismStrategy._13x1;
                                break;
                            case EOrganismStrategy._13x1:
                                Strategy = EOrganismStrategy._14x1;
                                break;
                            case EOrganismStrategy._14x1:
                                Strategy = EOrganismStrategy._2x6;
                                break;
                            case EOrganismStrategy._2x6:
                                Strategy = EOrganismStrategy._2x7;
                                break;
                            case EOrganismStrategy._2x7:
                                Strategy = EOrganismStrategy._6x2;
                                break;
                            case EOrganismStrategy._6x2:
                                Strategy = EOrganismStrategy._7x2;
                                break;
                            case EOrganismStrategy._7x2:
                                Strategy = EOrganismStrategy._3x4;
                                break;
                            case EOrganismStrategy._3x4:
                                Strategy = EOrganismStrategy._4x3;
                                break;
                            case EOrganismStrategy._4x3: // this was the last availble strategy
                                Strategy = EOrganismStrategy.none; // no more available strategies
                                Status = ECellStatus.dead; // this cell/organsim cannot exist
                                return;
                            default:
                                Status = ECellStatus.inactive;
                                Strategy = EOrganismStrategy.none;
                                Reset();
                                return;
                        }
                        Status = ECellStatus.active;
                        Reset();
                        OrganismId = Id;
                    }
                    else
                    {
                        Status = ECellStatus.inactive;
                        Strategy = EOrganismStrategy.none;
                        Reset();
                    }
                    break;
                case ECellStatus.valid: // the organism has fullfilled its purpose, for now
                    break;
                case ECellStatus.dead: // the cell is dead it cannot be part of an organism
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private ECellStatus DeterminCellStatus(bool isValidCandidate)
        {
            ECellStatus status = ECellStatus.inactive;

            if (!isValidCandidate)
                return status = ECellStatus.invalid;

            switch (OrganismStatus)
            {
                case EOrganismStatus.growing:
                    status = ECellStatus.active;
                    break;
                case EOrganismStatus.stable:
                    status = ECellStatus.valid;
                    break;
                case EOrganismStatus.dying:
                    status = ECellStatus.invalid;
                    break;
                default:
                    throw new NotImplementedException();
            }

            return status;
        }

        /// <summary>
        /// this function is very complex and can use some refactoring once it works
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private Cell GetSuccessor()
        {
            if (Successor != null)
                return CellMap[Successor.R][Successor.C];

            Cell successor = null;

            // the neighbour's strategy contains the info required to see if 
            //List<List<int>> matrix = new List<List<int>>();
            switch (Strategy)
            {
                case EOrganismStrategy._1x14:
                    successor = FindSuccessor(this, 1, 14);
                    break;
                case EOrganismStrategy._1x13:
                    successor = FindSuccessor(this, 1, 13);
                    break;
                case EOrganismStrategy._1x12:
                    successor = FindSuccessor(this, 1, 12);
                    break;
                case EOrganismStrategy._2x7:
                    successor = FindSuccessor(this, 2, 7);
                    break;
                case EOrganismStrategy._2x6:
                    successor = FindSuccessor(this, 2, 6);
                    break;
                case EOrganismStrategy._3x4:
                    successor = FindSuccessor(this, 3, 4);
                    break;
                case EOrganismStrategy._14x1:
                    successor = FindSuccessor(this, 14, 1);
                    break;
                case EOrganismStrategy._13x1:
                    successor = FindSuccessor(this, 13, 1);
                    break;
                case EOrganismStrategy._12x1:
                    successor = FindSuccessor(this, 12, 1);
                    break;
                case EOrganismStrategy._7x2:
                    successor = FindSuccessor(this, 7, 2);
                    break;
                case EOrganismStrategy._6x2:
                    successor = FindSuccessor(this, 6, 2);
                    break;
                case EOrganismStrategy._4x3:
                    successor = FindSuccessor(this, 4, 3);
                    break;
            }
            if (successor != null)
            {
                //Successor = new Position() { Y = successor.Position.X, X = successor.Position.Y };
                //IndexOrder = successor.IndexOrder - 1;
                return successor;
            }
            else
            {
                this.Status = ECellStatus.invalid;
            }
            return null;
        }

        //private Cell FindPredecessor(Cell neighbour, int maxX, int maxY)
        //{
        //    List<List<int>> matrix = PatternMap[neighbour.Strategy];

        //    for (int x = neighbour.Position.X - 1; x <= neighbour.Position.X + 1; x++)
        //    {
        //        for (int y = neighbour.Position.Y - 1; y <= neighbour.Position.Y + 1; y++)
        //        {
        //            if (x >= 0 && x < maxX &&
        //                y >= 0 && y < maxY &&
        //                matrix[x][y] == neighbour.IndexOrder - 1)
        //            {
        //                return cellMap[neighbour.Position.X + x][neighbour.Position.Y + y];
        //            }
        //        }
        //    }

        //    return null;
        //}


        private Cell FindSuccessor(Cell neighbour, int maxR, int maxC)
        {
            int nb_r = 0;
            int nb_c = 0;
            bool located = false;
            for (int r = 0; r < maxR; r++)
            {
                for (int c = 0; c < maxC; c++)
                {
                    if (neighbour.PatternMatrix[r][c] == neighbour.IndexOrder)
                    {
                        nb_r = r;
                        nb_c = c;
                        located = true;
                        break;
                    }
                }
                if (located) break;
            }
            if (!located)
                return null;

            for (int r = nb_r - 1; r <= nb_r + 1; r++)
            {
                for (int c = nb_c - 1; c <= nb_c + 1; c++)
                {
                    if (c >= 0 && c < maxC &&
                        r >= 0 && r < maxR &&
                        neighbour.PatternMatrix[r][c] == neighbour.IndexOrder + 1)
                    {
                        int nr = neighbour.Position.R + r - nb_r;
                        int nc = neighbour.Position.C + c - nb_c;

                        if (nr >= 0 && nr < MAX_R && nc >= 0 && nc < MAX_R)
                        {
                            return CellMap[nr][nc];
                        }
                        else
                            return null;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// this function is very complex and can use some refactoring once it works
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private Cell FindPredecessorOfHostOrganism()
        {
            if (Predecessor != null)
                return CellMap[Predecessor.R][Predecessor.C];

            Cell predecessor = null;
            Cell successor = null;
            for (int r = Position.R - 1; r <= Position.R + 1; r++)
            {
                for (int c = Position.C - 1; c <= Position.C + 1; c++)
                {
                    if (r >= 0 && r < MAX_R &&
                        c >= 0 && c < MAX_C)
                    {
                        Cell neighbour = CellMap[r][c];

                        if (neighbour.Status != ECellStatus.active)
                            continue;
                        // the neighbour's strategy contains the info required to see if 
                        switch (neighbour.Strategy)
                        {
                            case EOrganismStrategy._1x14:
                                successor = FindSuccessor(neighbour, 1, 14);
                                if (successor == null ||
                                    !(successor.Position.C == Position.C && successor.Position.R == Position.R))
                                    continue;
                                else
                                    predecessor = neighbour;
                                break;
                            case EOrganismStrategy._1x13:
                                successor = FindSuccessor(neighbour, 1, 13);
                                if (successor == null ||
                                    !(successor.Position.C == Position.C && successor.Position.R == Position.R))
                                    continue;
                                else
                                    predecessor = neighbour;
                                break;
                            case EOrganismStrategy._1x12:
                                successor = FindSuccessor(neighbour, 1, 12);
                                if (successor == null ||
                                    !(successor.Position.C == Position.C && successor.Position.R == Position.R))
                                    continue;
                                else
                                    predecessor = neighbour;
                                break;
                            case EOrganismStrategy._2x7:
                                successor = FindSuccessor(neighbour, 2, 7);
                                if (successor == null ||
                                    !(successor.Position.C == Position.C && successor.Position.R == Position.R))
                                    continue;
                                else
                                    predecessor = neighbour;
                                break;
                            case EOrganismStrategy._2x6:
                                successor = FindSuccessor(neighbour, 2, 6);
                                if (successor == null ||
                                    !(successor.Position.C == Position.C && successor.Position.R == Position.R))
                                    continue;
                                else
                                    predecessor = neighbour;
                                break;
                            case EOrganismStrategy._3x4:
                                successor = FindSuccessor(neighbour, 3, 4);
                                if (successor == null ||
                                    !(successor.Position.C == Position.C && successor.Position.R == Position.R))
                                    continue;
                                else
                                    predecessor = neighbour;
                                break;
                            case EOrganismStrategy._14x1:
                                successor = FindSuccessor(neighbour, 14, 1);
                                if (successor == null ||
                                    !(successor.Position.C == Position.C && successor.Position.R == Position.R))
                                    continue;
                                else
                                    predecessor = neighbour;
                                break;
                            case EOrganismStrategy._13x1:
                                successor = FindSuccessor(neighbour, 13, 1);
                                if (successor == null ||
                                    !(successor.Position.C == Position.C && successor.Position.R == Position.R))
                                    continue;
                                else
                                    predecessor = neighbour;
                                break;
                            case EOrganismStrategy._12x1:
                                successor = FindSuccessor(neighbour, 12, 1);
                                if (successor == null ||
                                    !(successor.Position.C == Position.C && successor.Position.R == Position.R))
                                    continue;
                                else
                                    predecessor = neighbour;
                                break;
                            case EOrganismStrategy._7x2:
                                successor = FindSuccessor(neighbour, 7, 2);
                                if (successor == null ||
                                    !(successor.Position.C == Position.C && successor.Position.R == Position.R))
                                    continue;
                                else
                                    predecessor = neighbour;
                                break;
                            case EOrganismStrategy._6x2:
                                successor = FindSuccessor(neighbour, 6, 2);
                                if (successor == null ||
                                    !(successor.Position.C == Position.C && successor.Position.R == Position.R))
                                    continue;
                                else
                                    predecessor = neighbour;
                                break;
                            case EOrganismStrategy._4x3:
                                successor = FindSuccessor(neighbour, 4, 3);
                                if (successor == null ||
                                    !(successor.Position.C == Position.C && successor.Position.R == Position.R))
                                    continue;
                                else
                                    predecessor = neighbour;
                                break;
                            default:
                                continue;
                        }
                        if (predecessor != null)
                        {
                            //Predecessor = new Position() { X = predecessor.Position.X, Y = predecessor.Position.Y };
                            IndexOrder = predecessor.IndexOrder + 1;
                            return predecessor;
                        }
                        return null;

                    }
                }
            }
            return null;
        }

        private bool DetermineActivation(out Position neighbour)
        {
            throw new NotImplementedException();
        }

        private bool DeteremineIfMotherCell()
        {
            if (Id == 0 && Status == ECellStatus.inactive) return true; // initial setup state 
            return OrganismId == Id && Status == ECellStatus.inactive;
        }

        public void Reset()
        {
            OrganismId = -1;
            Score = Content == Ingredient.Tomato ? 1 : -1;
            Size = 1;
            Predecessor = null;
            Successor = null;
        }

        public override string ToString()
        {
            return string.Format("({0}-{1}) i:{2} - {3} - {4} - Score:{5}", Position.R, Position.C, IndexOrder, Status.ToString(), Strategy.ToString(), Score);
        }
    }
}


