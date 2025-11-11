using Escaper.Persistence;
using System;
using System.Linq;
using System.Timers;

namespace Escaper.Model
{
    public class GameController : IGameController
    {
        private readonly Board _board;
        private System.Timers.Timer _timer;
        private int _elapsedTime;

        public bool IsGameOver { get; private set; } = false;
        public bool PlayerWon { get; private set; } = false;
        public int ElapsedTime => _elapsedTime;

        public event Action? BoardUpdated;
        public event Action? GameEnded;

        public GameController(Board board)
        {
            _board = board;

            _timer = new System.Timers.Timer(500);
            _timer.Elapsed += TimerElapsed!;
            _timer.AutoReset = true;
        }

        public void StartGame()
        {
            _elapsedTime = 0;
            IsGameOver = false;
            PlayerWon = false;
            _timer.Start();
        }

        public void PauseGame() => _timer.Stop();
        public void ResumeGame() => _timer.Start();

        private void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (IsGameOver) return;

            MoveEnemies();
            _elapsedTime++;
            BoardUpdated?.Invoke();

            if (IsGameOver)
                GameEnded?.Invoke();
        }

        public void MovePlayer(int dx, int dy)
        {
            if (IsGameOver) return;

            var newPos = new Position(_board.Player.Pos.X + dx, _board.Player.Pos.Y + dy);
            if (newPos.X < 0 || newPos.Y < 0 || newPos.X >= _board.Size || newPos.Y >= _board.Size) return;

            _board.Player.Pos = newPos;
            CheckCollisions();
            BoardUpdated?.Invoke();

            if (IsGameOver)
                GameEnded?.Invoke();
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
            if (_board.Mines.Any(m => m.Pos.Equals(_board.Player.Pos)) ||
                _board.Enemies.Any(e => e.IsActive && e.Pos.Equals(_board.Player.Pos)))
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
