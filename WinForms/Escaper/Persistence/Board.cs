namespace Escaper.Persistence
{
    public class Board
    {
        public int Size { get; set; }
        public Player Player { get; set; }
        public List<Enemy> Enemies { get; set; }
        public List<Mine> Mines { get; set; }

        public Board()
        {
            Size = 11;
            Player = new(new Position(Size / 2, 0));
            Enemies = [];
            Mines = [];
        }

        public Board(int size, int mineCount = 10)
        {
            Size = size;
            Player = new(new Position(size / 2, 0));
            Enemies = new()
            {
                new(new Position(0, size - 1)),
                new(new Position(size - 1, size - 1))
            };
            Mines = [];
            GenerateMines(mineCount);
        }

        private void GenerateMines(int count)
        {
            Random rnd = new();
            for (int i = 0; i < count; i++)
            {
                Position pos;
                do
                {
                    pos = new(rnd.Next(Size), rnd.Next(Size));
                }
                while (Mines.Any(m => m.Pos.Equals(pos))
                       || Player.Pos.Equals(pos)
                       || Enemies.Any(e => e.Pos.Equals(pos)));

                Mines.Add(new(pos));
            }
        }
    }
}
