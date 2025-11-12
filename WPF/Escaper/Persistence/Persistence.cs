using Escaper.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Escaper.Persistance
{
    public class Persistence : IPersistence
    {
        public void SaveGame(Board board, string path)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(board, options);
            File.WriteAllText(path, json);
        }

        public Board LoadGame(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Save file not found: {path}");

            string json = File.ReadAllText(path);
            var board = JsonSerializer.Deserialize<Board>(json);

            //hogy ne legyen possible null reference
            if (board is null)
                throw new InvalidOperationException("Failed to load game");

            return board;
        }

    }
}
