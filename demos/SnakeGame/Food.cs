using System;
using SnakeGame.Models;

namespace SnakeGame
{
    public class Food
    {
        private Position _foodPosition;

        public Food(int gridSize, int offset = 0)
        {
            this._foodPosition = new Position(gridSize / 4 + offset, gridSize / 4);
            DrawFood();
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

            DrawFood();
        }

        private void DrawFood()
        {
            Console.SetCursorPosition(this._foodPosition.X, this._foodPosition.Y);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("█");
        }
    }
}