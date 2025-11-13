using Escaper.Model;
using Escaper.Persistance;
using Escaper.Persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace EscaperTest
{
    [TestClass]
    public class EscaperModelTests : IDisposable
    {
        private Board _board = null!;
        private GameController _controller = null!;
        private Persistence? _persistence;

        [TestInitialize]
        public void Setup()
        {
            int size = 11;
            _board = new Board(size);
            _board.Mines.Add(new Mine(new Position(size / 2, size / 2)));

            _controller = new GameController(_board);
            _persistence = new Persistence();
        }

        [TestMethod]
        public void Controller_StartState_IsCorrect()
        {
            Assert.IsFalse(_controller.IsGameOver);
            Assert.IsFalse(_controller.PlayerWon);
            Assert.AreEqual(2, _controller.GetBoard().Enemies.Count);
        }

        [TestMethod]
        public void Player_Move_Right_UpdatesPosition()
        {
            var startPos = _board.Player.Pos;
            _controller.MovePlayer(1, 0);

            Assert.AreEqual(startPos.X + 1, _board.Player.Pos.X);
            Assert.AreEqual(startPos.Y, _board.Player.Pos.Y);
            Assert.IsFalse(_controller.IsGameOver);
        }

        [TestMethod]
        public void Player_Move_OutOfBounds_StaysInPlace()
        {
            _board.Player.Pos = new Position(0, 0);
            _controller.MovePlayer(-1, 0);

            Assert.AreEqual(0, _board.Player.Pos.X);
            Assert.AreEqual(0, _board.Player.Pos.Y);
        }

        [TestMethod]
        public void Player_StepsOnMine_GameEnds()
        {
            _board.Player.Pos = new Position(0, 2);
            _board.Mines.Add(new Mine(new Position(1, 2)));

            _controller.MovePlayer(1, 0);

            Assert.IsTrue(_controller.IsGameOver);
            Assert.IsFalse(_controller.PlayerWon);
        }

        [TestMethod]
        public void Enemy_MovesCloserToPlayer()
        {
            var enemy = _board.Enemies.First();
            var startPos = enemy.Pos;

            _controller.MoveEnemies();

            int oldDist = Math.Abs(startPos.X - _board.Player.Pos.X) + Math.Abs(startPos.Y - _board.Player.Pos.Y);
            int newDist = Math.Abs(enemy.Pos.X - _board.Player.Pos.X) + Math.Abs(enemy.Pos.Y - _board.Player.Pos.Y);

            Assert.IsTrue(newDist < oldDist);
        }

        [TestMethod]
        public void Enemy_StepsOnMine_BecomesInactive()
        {
            _board.Enemies.Clear();
            var enemy = new Enemy(new Position(2, 2));
            _board.Enemies.Add(enemy);

            _board.Mines.Add(new Mine(new Position(2, 3)));
            _board.Player.Pos = new Position(2, 3);

            _controller.MoveEnemies();

            Assert.IsFalse(enemy.IsActive);
        }

        [TestMethod]
        public void Enemy_CatchesPlayer_GameEnds()
        {
            _board.Enemies.Clear();
            var enemy = new Enemy(new Position(2, 1));
            _board.Enemies.Add(enemy);
            _board.Player.Pos = new Position(2, 0);

            _controller.MoveEnemies();

            Assert.IsTrue(_controller.IsGameOver);
            Assert.IsFalse(_controller.PlayerWon);
        }

        [TestMethod]
        public void Player_Wins_WhenAllEnemiesInactive()
        {
            _board.Enemies.Clear();
            _board.Enemies.Add(new Enemy(new Position(1, 2)) { IsActive = false });
            _board.Enemies.Add(new Enemy(new Position(3, 4)) { IsActive = false });

            _controller.MoveEnemies();

            Assert.IsTrue(_controller.IsGameOver);
            Assert.IsTrue(_controller.PlayerWon);
        }

        [TestMethod]
        public void NoActions_AfterGameOver()
        {
            _board.Player.Pos = new Position(2, 0);
            _board.Enemies.Clear();
            _board.Enemies.Add(new Enemy(new Position(2, 0)));

            _controller.MoveEnemies();
            Assert.IsTrue(_controller.IsGameOver);

            var oldPos = _board.Player.Pos;
            _controller.MovePlayer(1, 0);

            Assert.AreEqual(oldPos, _board.Player.Pos);
        }

        // ----------------------
        // Persistence teszt
        // ----------------------
        [TestMethod]
        public void Persistence_SaveAndLoadBoard_WorksCorrectly()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), "test_board.json");

            try
            {
                // ment
                _persistence!.SaveGame(_board, tempFile);

                // betölt
                var loadedBoard = _persistence.LoadGame(tempFile);

                // méret, Player, Mine, Enemy
                Assert.AreEqual(_board.Size, loadedBoard.Size);
                Assert.AreEqual(_board.Player.Pos.X, loadedBoard.Player.Pos.X);
                Assert.AreEqual(_board.Player.Pos.Y, loadedBoard.Player.Pos.Y);

                Assert.AreEqual(_board.Mines.Count, loadedBoard.Mines.Count);
                for (int i = 0; i < _board.Mines.Count; i++)
                {
                    Assert.AreEqual(_board.Mines[i].Pos.X, loadedBoard.Mines[i].Pos.X);
                    Assert.AreEqual(_board.Mines[i].Pos.Y, loadedBoard.Mines[i].Pos.Y);
                }

                Assert.AreEqual(_board.Enemies.Count, loadedBoard.Enemies.Count);
                for (int i = 0; i < _board.Enemies.Count; i++)
                {
                    Assert.AreEqual(_board.Enemies[i].Pos.X, loadedBoard.Enemies[i].Pos.X);
                    Assert.AreEqual(_board.Enemies[i].Pos.Y, loadedBoard.Enemies[i].Pos.Y);
                    Assert.AreEqual(_board.Enemies[i].IsActive, loadedBoard.Enemies[i].IsActive);
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        public void Dispose()
        {
            _controller?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
