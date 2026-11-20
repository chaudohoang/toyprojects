using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SnakeGame
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var game = new SnakeConsoleGame();
            game.Run();
        }
    }

    public class SnakeConsoleGame
    {
        private readonly int width = 40;
        private readonly int height = 20;
        private readonly LinkedList<Point> snake = new LinkedList<Point>();
        private Direction direction = Direction.Right;
        private Point food;
        private Random random = new Random();
        private int score = 0;
        private bool gameOver = false;

        private enum Direction { Up, Down, Left, Right }

        private struct Point { public int X; public int Y; }

        public void Run()
        {
            Console.CursorVisible = false;
            try { Console.Title = "Snake - by Evelyn for Boss <3"; } catch { /* ignore */ }

            Initialize();

            while (!gameOver)
            {
                InputTick();
                Update();
                Draw();
                Thread.Sleep(Math.Max(40, 140 - Math.Min(score * 3, 90)));
            }

            Console.Clear();
            Console.SetCursorPosition(width / 2 - 8, height / 2);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Game Over! Score: {0}", score);
            Console.ResetColor();
            Console.SetCursorPosition(0, height);
            Console.WriteLine("\nNhan phim bat ky de ket thuc...");
            Console.ReadKey(true);
        }

        private void Initialize()
        {
            snake.Clear();
            for (int i = 4; i >= 0; i--)
                snake.AddFirst(new Point { X = 10 + i, Y = 10 });

            direction = Direction.Right;
            score = 0;
            gameOver = false;
            SpawnFood();
        }

        private void InputTick()
        {
            while (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.W: case ConsoleKey.UpArrow: if (direction != Direction.Down) direction = Direction.Up; break;
                    case ConsoleKey.S: case ConsoleKey.DownArrow: if (direction != Direction.Up) direction = Direction.Down; break;
                    case ConsoleKey.A: case ConsoleKey.LeftArrow: if (direction != Direction.Right) direction = Direction.Left; break;
                    case ConsoleKey.D: case ConsoleKey.RightArrow: if (direction != Direction.Left) direction = Direction.Right; break;
                    case ConsoleKey.Escape: gameOver = true; break;
                }
            }
        }

        private void Update()
        {
            var head = snake.First.Value;

            switch (direction)
            {
                case Direction.Up: head = new Point { X = head.X, Y = head.Y - 1 }; break;
                case Direction.Down: head = new Point { X = head.X, Y = head.Y + 1 }; break;
                case Direction.Left: head = new Point { X = head.X - 1, Y = head.Y }; break;
                case Direction.Right: head = new Point { X = head.X + 1, Y = head.Y }; break;
            }

            // Wall collision
            if (head.X < 0 || head.Y < 0 || head.X >= width || head.Y >= height)
            {
                gameOver = true;
                return;
            }

            // Self collision
            if (snake.Any(p => p.X == head.X && p.Y == head.Y))
            {
                gameOver = true;
                return;
            }

            snake.AddFirst(head);

            if (head.X == food.X && head.Y == food.Y)
            {
                score += 10;
                SpawnFood();
            }
            else
            {
                snake.RemoveLast();
            }
        }

        private void Draw()
        {
            Console.SetCursorPosition(0, 0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (y == 0 || y == height - 1 || x == 0 || x == width - 1)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write('#');
                        continue;
                    }

                    if (y == food.Y && x == food.X)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write('$');
                        continue;
                    }

                    bool isSnake = snake.Any(p => p.X == x && p.Y == y);

                    if (isSnake)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write('o');
                        continue;
                    }

                    Console.Write(' ');
                }
            }

            Console.SetCursorPosition(0, height);
            Console.ResetColor();
            Console.Write("Score: {0}  |  W/A/S/D hoac mui ten de di chuyen  |  ESC de thoat", score);
        }

        private void SpawnFood()
        {
            var occupied = new HashSet<(int X, int Y)>(snake.Select(p => (p.X, p.Y)));
            var candidates = new List<Point>();
            for (int x = 1; x < width - 1; x++)
                for (int y = 1; y < height - 1; y++)
                    if (!occupied.Contains((x, y)))
                        candidates.Add(new Point { X = x, Y = y });

            if (candidates.Count == 0)
            {
                gameOver = true;
                return;
            }

            var pick = candidates[random.Next(candidates.Count)];
            food = pick;
        }
    }
}
