using System;
using Aiursoft.SnakeGame.Models;

namespace Aiursoft.SnakeGame.Services.Implements
{
    public class Food : IDrawable
    {
        private Position _foodPosition;

        public Food(int gridSize, int offset = 0)
        {
            _foodPosition = new Position{ X = gridSize / 4 + offset, Y = gridSize / 4};
            Draw();
        }

        public Position GetFoodPosition()
        {
            return _foodPosition;
        }

        public void RandomFoodPosition(Grid grid,Snake snake)
        {
            while (_foodPosition == null || snake.OnSnake(_foodPosition))
            {
                _foodPosition = grid.RandomGridPosition();
            }

            Draw();
        }

        public void Draw()
        {
            Console.SetCursorPosition(_foodPosition.X, _foodPosition.Y);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("█");
        }
    }
}