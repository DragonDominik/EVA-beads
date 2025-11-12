using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escaper.Persistence
{
    public struct Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Position(int x, int y) { X = x; Y = y; }

        public bool Equals(Position other) => X == other.X && Y == other.Y;
    }
}
