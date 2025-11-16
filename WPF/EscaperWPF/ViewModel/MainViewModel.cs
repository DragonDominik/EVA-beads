using Escaper.Model;
using Escaper.Persistance;
using Escaper.Persistence;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EscaperWPF.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private IGameController? _logic;
        private readonly IPersistence? _persistence;
        private bool _isPaused = true;
        private int _cellSize = 30;
        public ObservableCollection<UIElement> BoardElements { get; } = [];
        public DelegateCommand NewGameCommand { get; }
        public DelegateCommand PauseCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand LoadCommand { get; }
        public DelegateCommand MovePlayerCommand { get; }

        public ObservableCollection<int> MapSizes { get; } = [11, 15, 21];

        private int _selectedMapSize = 21;
        public int SelectedMapSize
        {
            get => _selectedMapSize;
            set => SetProperty(ref _selectedMapSize, value);
        }

        // status, time
        private string _status = "Start a game";
        public string Status { get => _status; set => SetProperty(ref _status, value); }

        private string _time = "Time: 0";
        public string Time { get => _time; set => SetProperty(ref _time, value); }

        // canvas size
        private double _canvasWidth = 600;
        public double CanvasWidth
        {
            get => _canvasWidth;
            set => SetProperty(ref _canvasWidth, value);
        }

        private double _canvasHeight = 600;
        public double CanvasHeight
        {
            get => _canvasHeight;
            set => SetProperty(ref _canvasHeight, value);
        }

        public MainViewModel()
        {
            _persistence = new Persistence();

            // Commands
            NewGameCommand = new DelegateCommand(_ => StartNewGame());
            PauseCommand = new DelegateCommand(_ => TogglePause());
            SaveCommand = new DelegateCommand(_ => SaveGame());
            LoadCommand = new DelegateCommand(_ => LoadGame());

            MovePlayerCommand = new DelegateCommand(param =>
            {
                if (_logic == null || _isPaused) return;

                if (param is string direction)
                {
                    switch (direction)
                    {
                        case "Up": _logic.MovePlayer(0, -1); break;
                        case "Down": _logic.MovePlayer(0, 1); break;
                        case "Left": _logic.MovePlayer(-1, 0); break;
                        case "Right": _logic.MovePlayer(1, 0); break;
                    }
                }
            });
        }

        private void StartNewGame()
        {
            // Dispose previous game if exists
            if (_logic != null)
            {
                if (_logic is IDisposable disposable)
                    disposable.Dispose();
                _logic = null;
            }

            int size = SelectedMapSize;
            var board = new Board(size, size);
            _logic = new GameController(board);

            _logic.BoardUpdated += () => Application.Current.Dispatcher.Invoke(UpdateBoard);
            _logic.GameEnded += () => Application.Current.Dispatcher.Invoke(EndGame);

            _logic.StartGame();
            _isPaused = false;
            Status = "Game Running";
            Time = "Time: 0";

            UpdateBoard();
        }

        private void TogglePause()
        {
            if (_logic == null || _logic.IsGameOver) return;

            _isPaused = !_isPaused;
            if (_isPaused) (_logic as GameController)?.PauseGame();
            else (_logic as GameController)?.ResumeGame();

            Status = _isPaused ? "Game Paused" : "Game Running";
        }

        private void SaveGame()
        {
            if (_logic == null || _logic.IsGameOver) return;

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string path = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                $"escaper_{timestamp}.json"
            );

            _persistence!.SaveGame(_logic.GetBoard(), path);
            MessageBox.Show($"Game saved to:\n{path}");
        }

        private void LoadGame()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (ofd.ShowDialog() == true)
            {
                // dispose of prev game if exists
                if (_logic != null && _logic is IDisposable disposable)
                    disposable.Dispose();

                _logic = new GameController(_persistence!.LoadGame(ofd.FileName));
                _logic.BoardUpdated += () => Application.Current.Dispatcher.Invoke(UpdateBoard);
                _logic.GameEnded += () => Application.Current.Dispatcher.Invoke(EndGame);

                _isPaused = true;
                (_logic as GameController)?.PauseGame();

                Status = "Game Paused";
                Time = "Time: 0";

                UpdateBoard();
            }
        }

        private void EndGame()
        {
            (_logic as GameController)?.PauseGame();
            _isPaused = true;

            string result = _logic!.PlayerWon ? "You won" : "You lost";
            Status = result;

            int elapsed = (_logic as GameController)?.ElapsedTime ?? 0;
            MessageBox.Show($"{result} in {elapsed} seconds!", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateBoard()
        {
            if (_logic == null) return;

            var window = Application.Current?.MainWindow;
            if (window == null) return;

            var board = _logic.GetBoard();
            if (board?.Player?.Pos == null) return;

            var topPanel = window.FindName("TopPanel") as Panel;
            if (topPanel == null) return;

            double availableWidth = window.ActualWidth - 20;
            double availableHeight = window.ActualHeight - topPanel.ActualHeight - 40;

            _cellSize = (int)Math.Min(availableWidth / board.Size, availableHeight / board.Size);


            CanvasWidth = _cellSize * board.Size;
            CanvasHeight = _cellSize * board.Size;

            BoardElements.Clear();
            int margin = _cellSize / 5;

            // Cells
            for (int x = 0; x < board.Size; x++)
                for (int y = 0; y < board.Size; y++)
                {
                    var rect = new Rectangle
                    {
                        Width = _cellSize - 1,
                        Height = _cellSize - 1,
                        Stroke = Brushes.Black
                    };
                    Canvas.SetLeft(rect, x * _cellSize);
                    Canvas.SetTop(rect, y * _cellSize);
                    BoardElements.Add(rect);
                }

            // Mines
            foreach (var mine in board.Mines)
            {
                var rect = new Rectangle
                {
                    Width = _cellSize - 2 * margin,
                    Height = _cellSize - 2 * margin,
                    Fill = Brushes.Black
                };
                Canvas.SetLeft(rect, mine.Pos.X * _cellSize + margin);
                Canvas.SetTop(rect, mine.Pos.Y * _cellSize + margin);
                BoardElements.Add(rect);
            }

            // Player
            var p = board.Player.Pos;
            var playerEllipse = new Ellipse
            {
                Width = _cellSize - 2 * margin,
                Height = _cellSize - 2 * margin,
                Fill = Brushes.Blue
            };
            Canvas.SetLeft(playerEllipse, p.X * _cellSize + margin);
            Canvas.SetTop(playerEllipse, p.Y * _cellSize + margin);
            BoardElements.Add(playerEllipse);

            // Enemies
            foreach (var enemy in board.Enemies.Where(en => en.IsActive))
            {
                var eEllipse = new Ellipse
                {
                    Width = _cellSize - 2 * margin,
                    Height = _cellSize - 2 * margin,
                    Fill = Brushes.Red
                };
                Canvas.SetLeft(eEllipse, enemy.Pos.X * _cellSize + margin);
                Canvas.SetTop(eEllipse, enemy.Pos.Y * _cellSize + margin);
                BoardElements.Add(eEllipse);
            }

            int elapsed = _logic?.ElapsedTime ?? 0;
            Time = $"Time: {elapsed}";
        }

    }
}
