using System;
using System.Linq;
using System.Threading.Tasks;
using SnakeGame.Models;
using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;

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
            Repository<string> repoA = new Repository<string>();
            await new WebSocketRemote<string>(_endpointUrl).AttachAsync(repoA);
            Repository<string> repoB = new Repository<string>();
            await new WebSocketRemote<string>(_endpointUrl).AttachAsync(repoB);

            #endregion
            
            Console.Clear();
            
            #region Setup variables for player
            
            Position direction = new Position(0, 0);
            bool isGameOver = false;
            // Build grid
            Grid grid = new Grid(GRID_SIZE);
            
            // Build snake
            Snake snake = new Snake(new Position(GRID_SIZE / 2, GRID_SIZE / 2));

            // Add food
            Food food = new Food(GRID_SIZE);
            
            #endregion
            
            #region Setup variables for observer

            int offset = GRID_SIZE + GRID_SIZE / 2;
            int CountOfRemote = 0;
            Position directionB = new Position(0, 0);
            bool isGameOverB = false;
            
            // Build grid
            Grid gridB = new Grid(GRID_SIZE, offset);
            
            // Build snake
            Snake snakeB = new Snake(new Position(GRID_SIZE / 2 + offset, GRID_SIZE / 2));
            
            // Add food
            Food foodB = new Food(GRID_SIZE, offset);
            
            #endregion
            
            // Get input command
            ConsoleKey command = Console.ReadKey().Key;
            
            while (!isGameOver)
            {
                Input.GetInputDirection(command, direction);
                repoA.Commit(direction.X + "," + direction.Y);
                snake.Update(direction);

                GameDisplay(snake, grid, food, out isGameOver);

                #region Observer's pannel

                if (repoB.Commits.Count() > CountOfRemote && !isGameOverB)
                {
                    // Such an ugly way
                    string str = repoB.Head.Item;
                    string[] temp = str.Split(",");
                    directionB.X = Int32.Parse(temp[0]);
                    directionB.Y = Int32.Parse(temp[1]);
                    
                    CountOfRemote = repoB.Commits.Count();
                    
                    snakeB.Update(directionB);

                    GameDisplay(snakeB, gridB, foodB, out isGameOverB);
                }

                #endregion

                if (Console.KeyAvailable)
                {
                    command = Console.ReadKey().Key;
                }

                // Slow the game down
                System.Threading.Thread.Sleep(Convert.ToInt32(GAME_SPEED));
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
