using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatchOctocat
{
    public class Octocat
    {
        public int ColumnIndex
        {
            get;
            set;
        }

        public int RowIndex
        {
            get;
            set;
        }

        public int EscapeAtRow
        {
            get;
            set;
        }
        public int EscapeAtColumn
        {
            get;
            set;
        }

        public HashSet<Tuple<int, int>> EscapePath
        {
            get;
            set;
        }

        public bool IsEnclosed
        {
            get;
            set;
        }
    }
}
