using SnakeGame.Models;
using System;

namespace SnakeGame.Services.Implements
{
    public class Grid : IDrawable
    {
        private readonly int _gridSize;
        // Distance between left boundary and left of console
        private readonly int _offset;
        private readonly Random _random;

        public Grid(int gridSize, int offset = 0, int seed = 0)
        {
            this._gridSize = gridSize;
            this._offset = offset;
            this._random = seed != 0 ? new Random(seed) : new Random();
            Draw();
        }

        public Position RandomGridPosition()
        {
            return new Position{ X = _random.Next(_offset + 2, _offset + _gridSize - 2), Y = _random.Next(2, _gridSize - 2)};
        }

        public bool OutsideGrid(Position p)
        {
            return p.X <= _offset + 1 || p.X >= _offset + _gridSize || p.Y <= 1 || p.Y >= _gridSize;
        }

        public void Draw()
        {
            for (int i = 1; i <= _gridSize; i++)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(_offset + 1, i);
                Console.Write("█");
                Console.SetCursorPosition(_offset + _gridSize, i);
                Console.Write("█");
            }

            for (int i = 1; i <= _gridSize; i++)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(_offset + i, 1);
                Console.Write("█");
                Console.SetCursorPosition(_offset + i, _gridSize);
                Console.Write("█");
            }
        }
    }
}
