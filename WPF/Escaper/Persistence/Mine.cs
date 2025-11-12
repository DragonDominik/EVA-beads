using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escaper.Persistence
{
    public class Mine
    {
        public Position Pos { get; set; }

        public Mine(Position pos)
        {
            Pos = pos;
        }
    }
}
