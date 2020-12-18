using System;
using System.Linq;
using System.Threading.Tasks;
using SnakeGame.Models;
using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using SnakeGame.Services;
using SnakeGame.Services.Implements;

namespace SnakeGame
{
    class Program
    {
        #region Global variables
        
        public static decimal GAME_SPEED = 150; //ms
        public static int GRID_SIZE = 40;
        
        private const int _port = 15000;
        private static readonly string _endpointUrl = $"ws://localhost:{_port}/repo.ares";
        
        #endregion

        static async Task Main(string[] args)
        {
            #region Repos

            // Build repository
            Repository<Position> repoA = new Repository<Position>();
            await new WebSocketRemote<Position>(_endpointUrl).AttachAsync(repoA);
            Repository<Position> repoB = new Repository<Position>();
            await new WebSocketRemote<Position>(_endpointUrl).AttachAsync(repoB);

            #endregion

            Console.Clear();
            int seed = new Random().Next();

            #region Setup variables for player

            Position direction = new Position{ X = 0, Y = 0 };
            bool isGameOver = false;

            // Build grid
            Grid grid = new Grid(GRID_SIZE, seed);
            // Build snake
            Snake snake = new Snake(new Position{ X = GRID_SIZE / 2, Y = GRID_SIZE / 2 });
            // Add food
            Food food = new Food(GRID_SIZE);

            #endregion

            #region Setup variables for observer

            int offset = GRID_SIZE + GRID_SIZE / 2;
            IRecurrent<Snake, Position> rec = new SnakeRecurrent();
            bool isGameOverB = false;
            
            // Build grid
            Grid gridB = new Grid(GRID_SIZE, seed, offset);
            // Show Original Snake
            // IDrawable.Draw(new Position{ X = GRID_SIZE / 2 + offset, Y=GRID_SIZE / 2 }, ConsoleColor.DarkGreen);
            Snake snakeB = new Snake(new Position {X = GRID_SIZE / 2 + offset, Y = GRID_SIZE / 2});
            // Add food
            Food foodB = new Food(GRID_SIZE, offset);
            
            #endregion
            
            // Get input command
            ConsoleKey command = Console.ReadKey().Key;
            
            while (!isGameOver)
            {
                Input.GetInputDirection(command, direction);
                repoA.Commit(direction);
                snake.Update(direction);

                GameDisplay(snake, grid, food, out isGameOver);

                #region Observer's pannel
                
                // Recurrent from start
                snakeB = rec.Recurrent(new Snake(new Position{ X = GRID_SIZE / 2 + offset, Y=GRID_SIZE / 2 }, snakeB.Count), repoB);

                GameDisplay(snakeB, gridB, foodB, out isGameOverB);

                #endregion

                if (Console.KeyAvailable)
                {
                    command = Console.ReadKey().Key;
                }

                // Slow the game down
                System.Threading.Thread.Sleep(Convert.ToInt32(GAME_SPEED));
            }

            while (!isGameOverB)
            {
                // Recurrent from start
                snakeB = rec.Recurrent(new Snake(new Position{ X = GRID_SIZE / 2 + offset, Y=GRID_SIZE / 2 }, snakeB.Count), repoB);

                GameDisplay(snakeB, gridB, foodB, out isGameOverB);
            }
            
        }

        static void GameDisplay(Snake s, Grid g, Food f, out bool state)
        {
            state = false;
            // Detect if snake hits the boundary or itself
            if (g.OutsideGrid(s.Head) || s.SnakeIntersection())
            {
                state = true;
                Console.SetCursorPosition(21, 20);
                Console.WriteLine("Oops, the snake died");
            }

            // Detect when foods were eaten
            if (s.CanEat(f.GetFoodPosition()))
            {
                f.GetRandomFoodPosition(g, s);
                s.AddBody();
                // Make snake faster
                GAME_SPEED *= 0.95m;
            }
        }
    }
}
