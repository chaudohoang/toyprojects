using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SnakeGameWPF
{
    public partial class MainWindow : Window
    {
        // Matched to the pygame build: 38x26 cells of 20px = 760x520 playfield,
        // 12 fps start ramping to 24, 3-segment snake.
        private const int GridWidth = 38;
        private const int GridHeight = 26;
        private const int CellSize = 20;
        private const int StartFps = 12;
        private const int MaxFps = 24;
        private const int StartLength = 3;

        private enum Direction { Up, Down, Left, Right }
        private enum GameState { Menu, Playing, GameOver }

        private readonly List<(int X, int Y)> snake = new();
        private readonly Queue<Direction> pendingTurns = new();
        private readonly Random random = new();
        private readonly DispatcherTimer gameTimer;

        private (int X, int Y) food;
        private Direction direction = Direction.Right;
        private GameState state = GameState.Menu;
        private int score;
        private int bestScore;

        private static readonly Brush HeadBrush = Frozen(255, 215, 0);
        private static readonly Brush EyeBrush = Frozen(0, 0, 0);
        private static readonly Brush FoodBrush = Frozen(220, 50, 50);
        private static readonly Brush ShineBrush = Frozen(255, 120, 120);
        private static readonly Dictionary<byte, Brush> bodyBrushes = new();
        private static readonly Brush GlowBrush = MakeGlow();

        private static Brush Frozen(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }

        private static Brush MakeGlow()
        {
            var glow = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.5, 0.5),
                Center = new Point(0.5, 0.5),
                RadiusX = 0.5,
                RadiusY = 0.5
            };
            glow.GradientStops.Add(new GradientStop(Color.FromArgb(70, 255, 60, 60), 0.0));
            glow.GradientStops.Add(new GradientStop(Color.FromArgb(28, 255, 60, 60), 0.55));
            glow.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 60, 60), 1.0));
            glow.Freeze();
            return glow;
        }

        // Body fades from bright to dark along the snake, like the pygame build.
        private static Brush BodyBrush(byte g)
        {
            if (!bodyBrushes.TryGetValue(g, out var brush))
            {
                brush = Frozen(50, g, 50);
                bodyBrushes[g] = brush;
            }
            return brush;
        }

        public MainWindow()
        {
            InitializeComponent();

            gameTimer = new DispatcherTimer
            {
                Interval = IntervalForLength(StartLength)
            };
            gameTimer.Tick += GameTimer_Tick;

            ResetBoard();
            ShowMenu();
        }

        // pygame: speed = min(FPS + len(body) // 5, MAX_FPS), converted to a
        // tick interval. Same ramp, same ceiling.
        private static TimeSpan IntervalForLength(int length)
        {
            int fps = Math.Min(StartFps + length / 5, MaxFps);
            return TimeSpan.FromMilliseconds(1000.0 / fps);
        }

        private void ResetBoard()
        {
            snake.Clear();
            pendingTurns.Clear();

            int cx = GridWidth / 2;
            int cy = GridHeight / 2;
            for (int i = 0; i < StartLength; i++)
                snake.Add((cx - i, cy));

            direction = Direction.Right;
            score = 0;
            gameTimer.Interval = IntervalForLength(StartLength);

            UpdateHud();
            SpawnFood();
            DrawGame();
        }

        private void ShowMenu()
        {
            state = GameState.Menu;
            gameTimer.Stop();
            OverlayPanel.Visibility = Visibility.Collapsed;
            NewBestText.Visibility = Visibility.Collapsed;
            MenuPanel.Visibility = Visibility.Visible;
        }

        private void StartGame()
        {
            ResetBoard();
            state = GameState.Playing;
            MenuPanel.Visibility = Visibility.Collapsed;
            OverlayPanel.Visibility = Visibility.Collapsed;
            NewBestText.Visibility = Visibility.Collapsed;
            gameTimer.Start();
        }

        private void UpdateHud()
        {
            ScoreText.Text = $"Score: {score}";
            BestText.Text = $"Best: {bestScore}";
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (state != GameState.Playing) return;

            // One queued turn per tick, so a fast double-tap can't fold the
            // snake back into its own neck.
            if (pendingTurns.Count > 0)
                direction = pendingTurns.Dequeue();

            MoveSnake();
            CheckCollisions();
            DrawGame();
        }

        private static bool IsOpposite(Direction a, Direction b) =>
            (a == Direction.Up && b == Direction.Down) ||
            (a == Direction.Down && b == Direction.Up) ||
            (a == Direction.Left && b == Direction.Right) ||
            (a == Direction.Right && b == Direction.Left);

        private void TryQueueTurn(Direction next)
        {
            Direction last = pendingTurns.Count > 0 ? pendingTurns.Last() : direction;
            if (next == last || IsOpposite(next, last)) return;
            if (pendingTurns.Count < 2) pendingTurns.Enqueue(next);
        }

        private void MoveSnake()
        {
            var head = snake[0];

            head = direction switch
            {
                Direction.Up => (head.X, head.Y - 1),
                Direction.Down => (head.X, head.Y + 1),
                Direction.Left => (head.X - 1, head.Y),
                Direction.Right => (head.X + 1, head.Y),
                _ => head
            };

            snake.Insert(0, head);

            if (head == food)
            {
                score += 10;
                UpdateHud();
                SpawnFood();
                gameTimer.Interval = IntervalForLength(snake.Count);
            }
            else
            {
                snake.RemoveAt(snake.Count - 1);
            }
        }

        private void CheckCollisions()
        {
            var head = snake[0];

            if (head.X < 0 || head.X >= GridWidth || head.Y < 0 || head.Y >= GridHeight)
            {
                GameOver();
                return;
            }

            for (int i = 1; i < snake.Count; i++)
            {
                if (head == snake[i])
                {
                    GameOver();
                    return;
                }
            }
        }

        private void SpawnFood()
        {
            var occupied = new HashSet<(int X, int Y)>(snake);
            var candidates = new List<(int X, int Y)>();

            for (int x = 0; x < GridWidth; x++)
                for (int y = 0; y < GridHeight; y++)
                    if (!occupied.Contains((x, y)))
                        candidates.Add((x, y));

            if (candidates.Count == 0)
            {
                GameOver();
                return;
            }

            food = candidates[random.Next(candidates.Count)];
        }

        private void AddShape(Shape shape, double left, double top)
        {
            Canvas.SetLeft(shape, left);
            Canvas.SetTop(shape, top);
            GameCanvas.Children.Add(shape);
        }

        private void DrawFood()
        {
            double cx = food.X * CellSize + CellSize / 2.0;
            double cy = food.Y * CellSize + CellSize / 2.0;

            AddShape(new Ellipse
            {
                Width = CellSize * 2,
                Height = CellSize * 2,
                Fill = GlowBrush,
                IsHitTestVisible = false
            }, cx - CellSize, cy - CellSize);

            AddShape(new Ellipse
            {
                Width = CellSize - 2,
                Height = CellSize - 2,
                Fill = FoodBrush
            }, cx - (CellSize - 2) / 2.0, cy - (CellSize - 2) / 2.0);

            AddShape(new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = ShineBrush
            }, cx - 5, cy - 6);
        }

        private void DrawHead()
        {
            var (hx, hy) = snake[0];

            AddShape(new Rectangle
            {
                Width = CellSize - 2,
                Height = CellSize - 2,
                Fill = HeadBrush,
                RadiusX = 6,
                RadiusY = 6
            }, hx * CellSize + 1, hy * CellSize + 1);

            double cx = hx * CellSize + CellSize / 2.0;
            double cy = hy * CellSize + CellSize / 2.0;

            (double dx1, double dy1, double dx2, double dy2) = direction switch
            {
                Direction.Right => (4.0, -4.0, 4.0, 4.0),
                Direction.Left => (-4.0, -4.0, -4.0, 4.0),
                Direction.Up => (-4.0, -4.0, 4.0, -4.0),
                Direction.Down => (-4.0, 4.0, 4.0, 4.0),
                _ => (4.0, -4.0, 4.0, 4.0)
            };

            AddShape(new Ellipse { Width = 5, Height = 5, Fill = EyeBrush }, cx + dx1 - 2.5, cy + dy1 - 2.5);
            AddShape(new Ellipse { Width = 5, Height = 5, Fill = EyeBrush }, cx + dx2 - 2.5, cy + dy2 - 2.5);
        }

        private void DrawGame()
        {
            GameCanvas.Children.Clear();

            DrawFood();

            // Tail first so the head and its eyes sit on top.
            for (int i = snake.Count - 1; i >= 1; i--)
            {
                byte g = (byte)(200 - (double)i / snake.Count * 100);
                AddShape(new Rectangle
                {
                    Width = CellSize - 2,
                    Height = CellSize - 2,
                    Fill = BodyBrush(g),
                    RadiusX = 4,
                    RadiusY = 4
                }, snake[i].X * CellSize + 1, snake[i].Y * CellSize + 1);
            }

            DrawHead();
        }

        private void GameOver()
        {
            state = GameState.GameOver;
            gameTimer.Stop();

            bool isNewBest = score > bestScore && score > 0;
            if (isNewBest)
            {
                bestScore = score;
                UpdateHud();
            }

            FinalScoreText.Text = $"Score: {score}";
            NewBestText.Visibility = isNewBest ? Visibility.Visible : Visibility.Collapsed;
            OverlayPanel.Visibility = Visibility.Visible;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            switch (state)
            {
                case GameState.Menu:
                    if (e.Key == Key.Space || e.Key == Key.Enter) StartGame();
                    else if (e.Key == Key.Escape) Close();
                    else e.Handled = false;
                    return;

                case GameState.GameOver:
                    if (e.Key == Key.Space || e.Key == Key.Enter || e.Key == Key.R) StartGame();
                    else if (e.Key == Key.Escape) ShowMenu();
                    else e.Handled = false;
                    return;
            }

            switch (e.Key)
            {
                case Key.W:
                case Key.Up:
                    TryQueueTurn(Direction.Up);
                    break;
                case Key.S:
                case Key.Down:
                    TryQueueTurn(Direction.Down);
                    break;
                case Key.A:
                case Key.Left:
                    TryQueueTurn(Direction.Left);
                    break;
                case Key.D:
                case Key.Right:
                    TryQueueTurn(Direction.Right);
                    break;
                case Key.R:
                    StartGame();
                    break;
                case Key.Escape:
                    ShowMenu();
                    break;
                default:
                    e.Handled = false;
                    break;
            }
        }
    }
}
