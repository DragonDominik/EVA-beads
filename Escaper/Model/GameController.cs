using Escaper.Model;

namespace Escaper.Model
{
    public class GameController : IGameController
    {
        private readonly Board _board;
        public bool IsGameOver { get; private set; } = false;
        public bool PlayerWon { get; private set; } = false;

        public GameController(Board board) => _board = board;

        public void MovePlayer(int dx, int dy)
        {
            if (IsGameOver) return;

            var newPos = new Position(_board.Player.Pos.X + dx, _board.Player.Pos.Y + dy);
            if (newPos.X < 0 || newPos.Y < 0 || newPos.X >= _board.Size || newPos.Y >= _board.Size) return;

            _board.Player.Pos = newPos;
            CheckCollisions();
        }

        public void MoveEnemies()
        {
            foreach (var enemy in _board.Enemies.Where(e => e.IsActive))
            {
                int dx = _board.Player.Pos.X - enemy.Pos.X;
                int dy = _board.Player.Pos.Y - enemy.Pos.Y;

                if (Math.Abs(dy) > Math.Abs(dx))
                    enemy.Pos = new Position(enemy.Pos.X, enemy.Pos.Y + Math.Sign(dy));
                else
                    enemy.Pos = new Position(enemy.Pos.X + Math.Sign(dx), enemy.Pos.Y);

                if (_board.Mines.Any(m => m.Pos.Equals(enemy.Pos)))
                    enemy.IsActive = false;
            }

            CheckCollisions();
        }

        private void CheckCollisions()
        {
            if (_board.Mines.Any(m => m.Pos.Equals(_board.Player.Pos)))
            {
                IsGameOver = true;
                PlayerWon = false;
            }

            if (_board.Enemies.Any(e => e.IsActive && e.Pos.Equals(_board.Player.Pos)))
            {
                IsGameOver = true;
                PlayerWon = false;
            }

            if (_board.Enemies.All(e => !e.IsActive))
            {
                IsGameOver = true;
                PlayerWon = true;
            }
        }

        public Board GetBoard() => _board;
    }
}