using Escaper.Model;
using Escaper.Persistance;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace escaperView
{
    public partial class MainForm : Form
    {
        private IGameController? _logic;
        private IPersistence _persistence;
        private System.Windows.Forms.Timer _gameTimer;
        private int _elapsedTime;
        private int _cellSize;
        private bool _isPaused = true;

        public MainForm()
        {
            InitializeComponent();
            _persistence = new Persistence();

            _gameTimer = new System.Windows.Forms.Timer { Interval = 500 };
            _gameTimer.Tick += GameTimer_Tick!;

            gameBoard.Paint += GameBoard_Paint!;
            gameBoard.GetType()
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(gameBoard, true, null);

            Resize += (s, e) => ResizeAndCenterGameBoard();
            labelStatus.Text = "Start a game";
        }

        private void NewGameBtn_Click(object sender, EventArgs e)
        {
            int size = mapSize.SelectedItem?.ToString() switch
            {
                "11x11" => 11,
                "15x15" => 15,
                "21x21" => 21,
                _ => 21
            };

            var board = new Board(size);

            Random rnd = new Random();
            for (int i = 0; i < size; i++)
            {
                Position pos;
                do
                {
                    pos = new Position(rnd.Next(size), rnd.Next(size));
                }
                while (board.Mines.Any(m => m.Pos.Equals(pos))
                    || board.Player.Pos.Equals(pos)
                    || board.Enemies.Any(e => e.Pos.Equals(pos)));

                board.Mines.Add(new Mine(pos));
            }

            _logic = new GameController(board);
            _elapsedTime = 0;
            labelTime.Text = "Time: 0";

            _isPaused = false;
            _gameTimer.Start();
            labelStatus.Text = "Game Running";

            ResizeAndCenterGameBoard();
            gameBoard.Invalidate();
        }

        private void PauseBtn_Click(object sender, EventArgs e)
        {
            if (_logic == null) return;

            if (_logic.IsGameOver)
            {
                MessageBox.Show("Cannot pause or resume a finished game!");
                return;
            }

            _isPaused = !_isPaused;
            if (_isPaused)
                _gameTimer.Stop();
            else
                _gameTimer.Start();

            labelStatus.Text = _isPaused ? "Game Paused" : "Game Running";
        }


        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (_logic == null)
            {
                MessageBox.Show("No game to save!");
                return;
            }

            if (_logic.IsGameOver)
            {
                MessageBox.Show("Cannot save a finished game!");
                return;
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                $"escaper_{timestamp}.json"
            );

            _persistence.SaveGame(_logic.GetBoard(), path);
            MessageBox.Show($"Game saved to:\n{path}");
        }


        private void LoadBtn_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Title = "Load Game"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _logic = new GameController(_persistence.LoadGame(ofd.FileName));
                    _elapsedTime = 0;
                    labelTime.Text = "Time: 0";
                    _isPaused = true;
                    _gameTimer.Stop();
                    labelStatus.Text = "Game Paused";

                    ResizeAndCenterGameBoard();
                    gameBoard.Invalidate();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while loading:\n{ex.Message}");
                }
            }
        }

        private void EndGame()
        {
            _gameTimer.Stop();
            _isPaused = true;

            string result = _logic!.PlayerWon ? "You won" : "You lost";
            labelStatus.Text = result;

            MessageBox.Show($"{result} in {_elapsedTime} seconds!", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (_logic == null || _isPaused) return;

            _logic.MoveEnemies();
            _elapsedTime++;
            labelTime.Text = $"Time: {_elapsedTime}";
            gameBoard.Invalidate();

            if (_logic.IsGameOver)
                EndGame();
        }

        private void GameBoard_Paint(object sender, PaintEventArgs e)
        {
            if (_logic == null) return;

            var board = _logic.GetBoard();
            Graphics g = e.Graphics;

            for (int x = 0; x < board.Size; x++)
                for (int y = 0; y < board.Size; y++)
                    g.DrawRectangle(Pens.Black, x * _cellSize, y * _cellSize, _cellSize - 1, _cellSize - 1);

            int margin = _cellSize / 5;

            foreach (var mine in board.Mines)
                g.FillRectangle(Brushes.Black, mine.Pos.X * _cellSize + margin, mine.Pos.Y * _cellSize + margin, _cellSize - 2 * margin, _cellSize - 2 * margin);

            var p = board.Player.Pos;
            g.FillEllipse(Brushes.Blue, p.X * _cellSize + margin, p.Y * _cellSize + margin, _cellSize - 2 * margin, _cellSize - 2 * margin);

            foreach (var enemy in board.Enemies.Where(e => e.IsActive))
                g.FillEllipse(Brushes.Red, enemy.Pos.X * _cellSize + margin, enemy.Pos.Y * _cellSize + margin, _cellSize - 2 * margin, _cellSize - 2 * margin);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_logic == null || _isPaused)
                return base.ProcessCmdKey(ref msg, keyData);

            switch (keyData)
            {
                case Keys.Up: _logic.MovePlayer(0, -1); break;
                case Keys.Down: _logic.MovePlayer(0, 1); break;
                case Keys.Left: _logic.MovePlayer(-1, 0); break;
                case Keys.Right: _logic.MovePlayer(1, 0); break;
                default: return base.ProcessCmdKey(ref msg, keyData);
            }

            gameBoard.Invalidate();

            if (_logic.IsGameOver)
                EndGame();

            return true;
        }

        private void ResizeAndCenterGameBoard()
        {
            if (_logic == null) return;

            int boardSize = _logic.GetBoard().Size;
            int padding = 10;
            int reservedHeight = (menuBar?.Height ?? 0) + (statusStrip?.Height ?? 0) + 2 * padding;
            int availableWidth = ClientSize.Width - 2 * padding;
            int availableHeight = ClientSize.Height - reservedHeight;

            _cellSize = Math.Min(availableWidth / boardSize, availableHeight / boardSize);
            int panelSize = _cellSize * boardSize;

            gameBoard.Width = gameBoard.Height = panelSize;
            gameBoard.Left = (ClientSize.Width - panelSize) / 2;
            gameBoard.Top = (menuBar?.Height ?? 0) + padding + ((availableHeight - panelSize) / 2);

            gameBoard.Invalidate();
        }
    }
}
