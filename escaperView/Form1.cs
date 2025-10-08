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
        private IGameController logic;
        private IPersistence persistence;
        private System.Windows.Forms.Timer gameTimer;
        private int elapsedTime;
        private int cellSize;
        private bool isPaused = true;

        public MainForm()
        {
            InitializeComponent();
            persistence = new Persistence();

            gameTimer = new System.Windows.Forms.Timer { Interval = 500 };
            gameTimer.Tick += GameTimer_Tick;

            gameBoard.Paint += gameBoard_Paint;
            gameBoard.GetType().GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(gameBoard, true, null);

            Resize += (s, e) => ResizeAndCenterGameBoard();
            labelStatus.Text = "Game Paused";
        }

        private void newGameBtn_Click(object sender, EventArgs e)
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
                do pos = new Position(rnd.Next(size), rnd.Next(size));
                while (board.Mines.Any(m => m.Pos.Equals(pos)) || board.Player.Pos.Equals(pos) || board.Enemies.Any(e => e.Pos.Equals(pos)));
                board.Mines.Add(new Mine { Pos = pos });
            }

            logic = new GameController(board);
            elapsedTime = 0;
            labelTime.Text = "Time: 0";

            isPaused = false;
            gameTimer.Start();
            labelStatus.Text = "Game Running";

            ResizeAndCenterGameBoard();
            gameBoard.Invalidate();
        }

        private void pauseBtn_Click(object sender, EventArgs e)
        {
            if (logic == null) return;

            isPaused = !isPaused;
            if (isPaused) gameTimer.Stop();
            else gameTimer.Start();

            labelStatus.Text = isPaused ? "Game Paused" : "Game Running";
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            if (logic == null)
            {
                MessageBox.Show("No game to save!");
                return;
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // pl 20251008_153045
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"escaper_{timestamp}.json");

            persistence.SaveGame(logic.GetBoard(), path);
            MessageBox.Show($"Game saved to: {path}");
        }

        private void loadBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                ofd.Title = "Load Game";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        logic = new GameController(persistence.LoadGame(ofd.FileName));
                        elapsedTime = 0;
                        labelTime.Text = "Time: 0";
                        isPaused = true;
                        gameTimer.Stop();
                        labelStatus.Text = "Game Paused";
                        ResizeAndCenterGameBoard();
                        gameBoard.Invalidate();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error while loading: {ex.Message}");
                    }
                }
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (logic == null || isPaused) return;

            logic.MoveEnemies();
            elapsedTime++;
            labelTime.Text = $"Time: {elapsedTime}";
            gameBoard.Invalidate();

            if (logic.IsGameOver)
            {
                gameTimer.Stop();
                isPaused = true;
                labelStatus.Text = logic.PlayerWon ? "You won!" : "You lost!";
            }
        }

        private void gameBoard_Paint(object sender, PaintEventArgs e)
        {
            if (logic == null) return;

            var board = logic.GetBoard();
            Graphics g = e.Graphics;

            for (int x = 0; x < board.Size; x++)
                for (int y = 0; y < board.Size; y++)
                    g.DrawRectangle(Pens.Black, new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize));

            int margin = cellSize / 5;
            foreach (var mine in board.Mines)
                g.FillRectangle(Brushes.Black, mine.Pos.X * cellSize + margin, mine.Pos.Y * cellSize + margin, cellSize - 2 * margin, cellSize - 2 * margin);

            var p = board.Player.Pos;
            g.FillEllipse(Brushes.Blue, p.X * cellSize, p.Y * cellSize, cellSize, cellSize);

            foreach (var enemy in board.Enemies.Where(e => e.IsActive))
                g.FillEllipse(Brushes.Red, enemy.Pos.X * cellSize, enemy.Pos.Y * cellSize, cellSize, cellSize);

            g.DrawRectangle(new Pen(Color.Black, 4), 0, 0, gameBoard.Width - 1, gameBoard.Height - 1);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (logic == null || isPaused) return base.ProcessCmdKey(ref msg, keyData);

            switch (keyData)
            {
                case Keys.Up: logic.MovePlayer(0, -1); break;
                case Keys.Down: logic.MovePlayer(0, 1); break;
                case Keys.Left: logic.MovePlayer(-1, 0); break;
                case Keys.Right: logic.MovePlayer(1, 0); break;
            }

            gameBoard.Invalidate();

            if (logic.IsGameOver)
            {
                gameTimer.Stop();
                isPaused = true;
                labelStatus.Text = logic.PlayerWon ? "You won!" : "You lost!";
            }

            return true;
        }

        private void ResizeAndCenterGameBoard()
        {
            if (logic == null) return;

            int boardSize = logic.GetBoard().Size;
            int padding = 10;
            int reservedHeight = (menuBar?.Height ?? 0) + (statusStrip?.Height ?? 0) + 2 * padding;
            int availableWidth = ClientSize.Width - 2 * padding;
            int availableHeight = ClientSize.Height - reservedHeight;

            cellSize = Math.Min(availableWidth / boardSize, availableHeight / boardSize);
            int panelSize = cellSize * boardSize;

            gameBoard.Width = gameBoard.Height = panelSize;
            gameBoard.Left = (ClientSize.Width - panelSize) / 2;
            gameBoard.Top = (menuBar?.Height ?? 0) + padding + ((availableHeight - panelSize) / 2);

            gameBoard.Invalidate();
        }
    }
}
