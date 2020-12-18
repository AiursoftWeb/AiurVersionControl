using System;
using SnakeGame.Models;
using SnakeGame.Services;

namespace SnakeGame
{
    public class Food : IDrawable
    {
        private Position _foodPosition;

        public Food(int gridSize, int offset = 0)
        {
            this._foodPosition = new Position{ X = gridSize / 4 + offset, Y = gridSize / 4};
            Draw();
        }

        public Position GetFoodPosition()
        {
            return this._foodPosition;
        }

        public void GetRandomFoodPosition(Grid grid,Snake snake)
        {
            while (this._foodPosition == null || snake.OnSnake(this._foodPosition))
            {
                this._foodPosition = grid.RandomGridPosition();
            }

            Draw();
        }

        public void Draw()
        {
            Console.SetCursorPosition(this._foodPosition.X, this._foodPosition.Y);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("█");
        }
    }
}