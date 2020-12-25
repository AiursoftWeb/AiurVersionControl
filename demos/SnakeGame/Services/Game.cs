using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using SnakeGame.Launcher;
using SnakeGame.Models;
using SnakeGame.Services.Implements;
using System;
using System.Threading.Tasks;
using Action = SnakeGame.Models.Action;

namespace SnakeGame.Services
{
    public class Game : IDrawable
    {
        public bool IsGameEnd { get; set; }
        private readonly Repository<Action> _repo;
        private readonly Position _direction = new();
        private readonly Grid _grid;
        private readonly Food _food;
        private readonly Position _originalSnakePosition;
        private readonly int _offset;
        private Snake _snake;
        private  ConsoleKey _command;
        private IRecurrent<Snake, Action> _rec;

        public Game(int gridSize, int offset)
        {
            this._repo = new Repository<Action>();
            this.IsGameEnd = false;
            this._offset = offset;
            this._grid = new Grid(gridSize, offset);
            this._originalSnakePosition = new Position{ X = gridSize / 2 + offset, Y = gridSize / 2 };
            this._snake = new Snake((Position)this._originalSnakePosition.Clone());
            this._food = new Food(gridSize, offset);
        }

        public async Task AddRemote(string endpointUrl)
        {
            await new WebSocketRemote<Action>(endpointUrl).AttachAsync(_repo);
        }

        public void Draw()
        {
            _grid.Draw();
            _snake.Draw();
            _food.Draw();
        }

        public void UpdateDirection()
        {
            Input.ChangeDirection(_command, _direction);
            
            this._snake.Update(_direction);
            this._repo.Commit(new Action{Type = ActionType.Move, Direction = _direction});
        }
        public void UpdateFrame()
        {
            CheckDeath();
            _snake.Draw();
        }

        public bool NeedSpeedUp()
        {
            return CheckEat();
        }

        public void GenerateRecurrent()
        {
            _rec = new SnakeRecurrent();
        }
        
        public void RecurrentFromRepo()
        {
            _snake = _rec.Recurrent(new Snake((Position)this._originalSnakePosition.Clone()), this._repo, _offset);
        }

        public void ListenInput(bool listenNow = false)
        {
            if (Console.KeyAvailable || listenNow)
            {
                _command = Console.ReadKey().Key;
            }
        }
        
        private void CheckDeath()
        {
            // Detect if snake hits the boundary or itself
            if (this._grid.OutsideGrid(_snake.Head) || this._snake.SnakeIntersection())
            {
                this.IsGameEnd = true;
                Console.SetCursorPosition(21, 20);
                Console.WriteLine("Oops, the snake died...");
            }
        }

        private bool CheckEat()
        {
            if (this._snake.CanEat(this._food.GetFoodPosition()))
            {
                this._food.RandomFoodPosition(this._grid, this._snake);
                this._snake.AddBody();
                this._repo.Commit(new Action{Type = ActionType.Eat, Direction = this._food.GetFoodPosition()});
                return true;
            }
            else 
                return false;
        }
    }
}