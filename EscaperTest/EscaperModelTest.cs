using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Escaper.Model;

namespace EscaperTest
{
    [TestClass]
    public class EscaperModelTests
    {
        private Board _board = null!;
        private GameController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            int size = 11; // 11x11-es pálya
            _board = new Board(size);

            // középre egy akna
            _board.Mines.Add(new Mine(new Position(size / 2, size / 2)));

            _controller = new GameController(_board);
        }

        [TestMethod]
        public void Controller_StartState_IsCorrect()
        {
            // Ellenőrizzük az alapállapotot
            Assert.IsFalse(_controller.IsGameOver);
            Assert.IsFalse(_controller.PlayerWon);
            Assert.AreEqual(2, _controller.GetBoard().Enemies.Count);
        }

        [TestMethod]
        public void Player_Move_Right_UpdatesPosition()
        {
            var startPos = _board.Player.Pos;
            _controller.MovePlayer(1, 0); // jobbra lép

            Assert.AreEqual(startPos.X + 1, _board.Player.Pos.X);
            Assert.AreEqual(startPos.Y, _board.Player.Pos.Y);
            Assert.IsFalse(_controller.IsGameOver);
        }

        [TestMethod]
        public void Player_Move_OutOfBounds_StaysInPlace()
        {
            _board.Player.Pos = new Position(0, 0);
            _controller.MovePlayer(-1, 0); // balra kilépne

            Assert.AreEqual(0, _board.Player.Pos.X);
            Assert.AreEqual(0, _board.Player.Pos.Y);
        }

        [TestMethod]
        public void Player_StepsOnMine_GameEnds()
        {
            _board.Player.Pos = new Position(0, 2);
            _board.Mines.Add(new Mine(new Position(1, 2))); // akna a következő lépésen

            _controller.MovePlayer(1, 0); // jobbra lép

            Assert.IsTrue(_controller.IsGameOver);
            Assert.IsFalse(_controller.PlayerWon);
        }

        [TestMethod]
        public void Enemy_MovesCloserToPlayer()
        {
            var enemy = _board.Enemies.First();
            var startPos = enemy.Pos;

            _controller.MoveEnemies();

            // ellenőrizzük hogy az ellenség közelebb került
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

            _board.Mines.Add(new Mine(new Position(2, 3))); // akna az ellenség előtt
            _board.Player.Pos = new Position(2, 3); // hogy az ellenség felé menjen

            _controller.MoveEnemies();

            Assert.IsFalse(enemy.IsActive); // ellenség inaktív lett
        }

        [TestMethod]
        public void Enemy_CatchesPlayer_GameEnds()
        {
            _board.Enemies.Clear();
            var enemy = new Enemy(new Position(2, 1));
            _board.Enemies.Add(enemy);
            _board.Player.Pos = new Position(2, 0);

            _controller.MoveEnemies(); // ellenség elkapja a játékost

            Assert.IsTrue(_controller.IsGameOver);
            Assert.IsFalse(_controller.PlayerWon);
        }

        [TestMethod]
        public void Player_Wins_WhenAllEnemiesInactive()
        {
            _board.Enemies.Clear();
            _board.Enemies.Add(new Enemy(new Position(1, 2)) { IsActive = false });
            _board.Enemies.Add(new Enemy(new Position(3, 4)) { IsActive = false });

            _controller.MoveEnemies(); // ellenőrizzük az állapotot

            Assert.IsTrue(_controller.IsGameOver);
            Assert.IsTrue(_controller.PlayerWon);
        }

        [TestMethod]
        public void NoActions_AfterGameOver()
        {
            _board.Player.Pos = new Position(2, 0);
            _board.Enemies.Clear();
            _board.Enemies.Add(new Enemy(new Position(2, 0))); // azonnal elkapja a játékost

            _controller.MoveEnemies();
            Assert.IsTrue(_controller.IsGameOver);

            var oldPos = _board.Player.Pos;
            _controller.MovePlayer(1, 0); // nem történik mozgás

            Assert.AreEqual(oldPos, _board.Player.Pos);
        }
    }
}
