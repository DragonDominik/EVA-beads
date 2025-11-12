using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escaper.Persistence
{
    public class Enemy
    {
        public Position Pos { get; set; }

        public bool IsActive { get; set; } = true;

        public Enemy(Position pos)
        {
            Pos = pos;
        }
    }
}
