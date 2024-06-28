using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnightMove2
{
    public class BoardCoordinate
    {
        public int XAxisPoint { get; set; }
        public int YAxisPoint { get; set; }

        public BoardCoordinate(int x, int y) 
        {
            this.XAxisPoint = x;
            this.YAxisPoint = y;
        }

    }
}
