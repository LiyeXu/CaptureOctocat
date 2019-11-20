using System;
using System.Collections.Generic;
using System.Linq;

namespace CatchOctocat
{
    public class Game
    {
        private Random Rand { get; set; } = new Random((int)DateTime.Now.Ticks);

        public int BoardSize { get; set; } = 21;
        public int NumOfCats { get; set; } = 3;
        
        public bool[,] Board
        {
            get;
            private set;
        }

        private Action OnCatMove
        {
            get;
            set;
        }

        public Octocat[] Cats
        {
            get;
            set;
        }
        
        public int Steps
        {
            get;
            set;
        }

        public void Reset()
        {
            Board = new bool[BoardSize, BoardSize];
            int numOfBlocks = Rand.Next(BoardSize * BoardSize / 5, BoardSize * BoardSize / 4);
            for (int i = 0; i < numOfBlocks; i++)
            {
                int row = Rand.Next(0, BoardSize);
                int column = Rand.Next(0, BoardSize);
                int j = 0;
                for (; j < NumOfCats; j++)
                {
                    bool isCatPosition = (row == BoardSize / 2) && (column == BoardSize / 2 - NumOfCats / 2 + j);
                    if (isCatPosition)
                    {
                        break;
                    }
                }
                if (j != NumOfCats)
                {
                    continue;
                }
                Board[row, column] = true;
            }
            Cats = new Octocat[NumOfCats];
            for (int i = 0; i < NumOfCats; i++)
            {
                Cats[i] = new Octocat()
                {
                    ColumnIndex = BoardSize / 2 - NumOfCats / 2 + i,
                    RowIndex = BoardSize / 2,
                    EscapeAtRow = -1,
                    EscapeAtColumn = -1,
                    EscapePath = new HashSet<Tuple<int, int>>()
                };
            }
            Steps = 0;
        }

        public Game(int boardSize, int numOfCats, Action onCatMove)
        {
            BoardSize = boardSize;
            NumOfCats = numOfCats;
            OnCatMove = onCatMove;
            Reset();
        }

        private bool IsEscaped(int row, int column)
        {
            int edge = BoardSize - 1;
            return column == 0 || row == 0 || column == edge || row == edge;
        }
        
        private bool CatMove()
        {
            int numOfFreeCat = 0;
            foreach(var cat in Cats)
            {
                int minDistance = FindMinimalEscapeDistance(cat);
                if (minDistance == int.MaxValue || minDistance == -1)
                {
                    cat.IsEnclosed = true;
                }
                else
                {
                    cat.IsEnclosed = false;
                }
                if (minDistance != -1)
                {
                    numOfFreeCat++;
                }
            }
            if (numOfFreeCat == 0)
            {
                return false;
            }
            OnCatMove();
            return true;
        }

        private bool[,] visited;
        private int[,] distance;
        private Tuple<int, int>[,] prev;
        
        private int FindMinimalEscapeDistance(Octocat cat)
        {
            int row = cat.RowIndex;
            int column = cat.ColumnIndex;
            if (IsEscaped(row, column))
            {
                return 1;
            }
            var queue = new Queue<Tuple<int, int>>();
            queue.Enqueue(new Tuple<int, int>(row, column));
            visited = new bool[BoardSize, BoardSize];
            distance = new int[BoardSize, BoardSize];
            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    distance[i, j] = int.MaxValue;
                }
            }
            prev = new Tuple<int,int>[BoardSize,BoardSize];
            distance[row, column] = 0;
            prev[row, column] = null;
            visited[row, column] = true;
            int ret = int.MaxValue;
            cat.EscapeAtRow = -1;
            cat.EscapeAtColumn = -1;
            var nextPos = new List<Tuple<int, int>>();            
            while (queue.Count() != 0)
            {
                var pos = queue.Dequeue();
                row = pos.Item1;
                column = pos.Item2;
                if (IsEscaped(row, column) && ret > distance[row, column])
                {
                    ret = distance[row, column];
                    cat.EscapeAtRow = row;
                    cat.EscapeAtColumn = column;
                }
                int newDistance = distance[row, column] + 1;
                bool isOdd = (row & 1) == 1;                
                int c2 = column + (isOdd ? 1 : -1);
                ProcessAdjacentNode(row - 1, column, queue, newDistance, pos, cat, nextPos);
                ProcessAdjacentNode(row - 1, c2, queue, newDistance, pos, cat, nextPos);
                ProcessAdjacentNode(row, column + 1, queue, newDistance, pos, cat, nextPos);
                ProcessAdjacentNode(row + 1, column, queue, newDistance, pos, cat, nextPos);
                ProcessAdjacentNode(row + 1, c2, queue, newDistance, pos, cat, nextPos);
                ProcessAdjacentNode(row, column - 1, queue, newDistance, pos, cat, nextPos);
            }
            if (ret == int.MaxValue)
            {
                cat.EscapePath.Clear();
                if (nextPos.Count != 0)
                {
                    int idx = Rand.Next(0, nextPos.Count - 1);
                    cat.RowIndex = nextPos[idx].Item1;
                    cat.ColumnIndex = nextPos[idx].Item2;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                cat.EscapePath.Clear();
                int u = cat.EscapeAtRow;
                int v = cat.EscapeAtColumn;
                var next = new Tuple<int, int>(u, v);
                cat.EscapePath.Add(next);
                var back = prev[u, v];
                back = prev[back.Item1, back.Item2];              
                while (back != null)
                {
                    back = prev[back.Item1, back.Item2];
                    next = prev[next.Item1, next.Item2];
                    cat.EscapePath.Add(next);
                }
                cat.RowIndex = next.Item1;
                cat.ColumnIndex = next.Item2;
            }
            return ret;
        }

        private void ProcessAdjacentNode(int row, int column, Queue<Tuple<int, int>> queue, int newDistance, Tuple<int, int> pos, Octocat cat, List<Tuple<int, int>> nextPos)
        {
            if (row >= 0 && 
                column >= 0 && 
                row < BoardSize && 
                column < BoardSize && 
                !Board[row, column] && 
                !visited[row, column] && 
                Cats.Count(cc => cc.RowIndex == row && cc.ColumnIndex == column) == 0)
            {
                var p = new Tuple<int, int>(row, column);
                queue.Enqueue(p);
                visited[row, column] = true;
                distance[row, column] = newDistance;
                prev[row, column] = pos;
                if (pos.Item1 == cat.RowIndex && pos.Item2 == cat.ColumnIndex)
                    nextPos.Add(p);
            }
        }

        public bool PlayerMove(int row, int column)
        {
            if (Board[row, column])
            {
                return true;
            }
            Board[row, column] = true;
            Steps++;
            if (!CatMove())
            {
                return false;
            }
            foreach (var cat in Cats)
            {
                if (IsEscaped(cat.RowIndex, cat.ColumnIndex))
                {
                    Steps = -1;
                    return false;
                }
            }
            return true;
        }
    }
}
