using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnightMove2
{
    public class CoordinateToCalculate
    {
        public int x { get; set; }
        public int y { get; set; }
        public int dist { get; set; }
        public List<string> path { get; set; }

        public CoordinateToCalculate(int x, int y, int dist, List<string> path)
        {
            this.x = x;
            this.y = y;
            this.dist = dist;
            this.path = path;
        }
    }
}
