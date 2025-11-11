using Escaper.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escaper.Model
{
    public interface IGameController
    {
        bool IsGameOver { get; }
        bool PlayerWon { get; }
        int ElapsedTime { get; }

        void MovePlayer(int dx, int dy);
        void StartGame();
        void PauseGame();
        void ResumeGame();
        Board GetBoard();

        event Action? BoardUpdated;
        event Action? GameEnded;
    }
}
