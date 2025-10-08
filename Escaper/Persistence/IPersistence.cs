using Escaper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escaper.Persistance
{
    public interface IPersistence
    {
        void SaveGame(Board board, string path);
        Board LoadGame(string path);
    }
}
