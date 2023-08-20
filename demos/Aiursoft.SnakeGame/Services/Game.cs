using Aiursoft.AiurEventSyncer.Models;
using Aiursoft.AiurEventSyncer.Remotes;
using Aiursoft.SnakeGame.Launcher;
using Aiursoft.SnakeGame.Models;
using Aiursoft.SnakeGame.Services.Implements;

namespace Aiursoft.SnakeGame.Services
{
    public class Game : IDrawable
    {
        public bool IsGameEnd { get; set; }
        private readonly Repository<Models.Action> _repo;
        private readonly Position _direction = new();
        private readonly Grid _grid;
        private readonly Food _food;
        private readonly Position _originalSnakePosition;
        private readonly int _offset;
        private Snake _snake;
        private  ConsoleKey _command;
        private IRecurrent<Snake, Models.Action> _rec;

        public Game(int gridSize, int offset)
        {
            _repo = new Repository<Models.Action>();
            IsGameEnd = false;
            _offset = offset;
            _grid = new Grid(gridSize, offset);
            _originalSnakePosition = new Position{ X = gridSize / 2 + offset, Y = gridSize / 2 };
            _snake = new Snake((Position)_originalSnakePosition.Clone());
            _food = new Food(gridSize, offset);
        }

        public async Task AddRemote(string endpointUrl)
        {
            await new WebSocketRemote<Models.Action>(endpointUrl).AttachAsync(_repo);
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
            
            _snake.Update(_direction);
            _repo.Commit(new Models.Action{Type = ActionType.Move, Direction = _direction});
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
            _snake = _rec.Recurrent(new Snake((Position)_originalSnakePosition.Clone()), _repo, _offset);
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
            if (_grid.OutsideGrid(_snake.Head) || _snake.SnakeIntersection())
            {
                IsGameEnd = true;
                Console.SetCursorPosition(21, 20);
                Console.WriteLine("Oops, the snake died...");
            }
        }

        private bool CheckEat()
        {
            if (_snake.CanEat(_food.GetFoodPosition()))
            {
                _food.RandomFoodPosition(_grid, _snake);
                _snake.AddBody();
                _repo.Commit(new Models.Action{Type = ActionType.Eat, Direction = _food.GetFoodPosition()});
                return true;
            }

            return false;
        }
    }
}