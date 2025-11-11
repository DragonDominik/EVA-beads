using Escaper.Persistence;

public class Board
{
    public int Size { get; set; }
    public Player Player { get; set; }
    public List<Enemy> Enemies { get; set; }
    public List<Mine> Mines { get; set; }

    // Paraméter nélküli konstruktor
    public Board()
    {
        Mines = new List<Mine>();
        Enemies = new List<Enemy>();
    }

    public Board(int size, int mineCount = 10)
    {
        Size = size;
        Player = new Player(new Position(size / 2, 0));
        Enemies = new List<Enemy>
        {
            new Enemy(new Position(0, size - 1)),
            new Enemy(new Position(size - 1, size - 1))
        };
        Mines = new List<Mine>();
        GenerateMines(mineCount);
    }

    private void GenerateMines(int count)
    {
        Random rnd = new Random();
        for (int i = 0; i < count; i++)
        {
            Position pos;
            do
            {
                pos = new Position(rnd.Next(Size), rnd.Next(Size));
            }
            while (Mines.Any(m => m.Pos.Equals(pos))
                   || Player.Pos.Equals(pos)
                   || Enemies.Any(e => e.Pos.Equals(pos)));

            Mines.Add(new Mine(pos));
        }
    }
}
