using System;
using SnakeGame.Models;

namespace SnakeGame.Services
{
    public interface IDrawable
    {
        void Draw();

        static void Draw(Position p, ConsoleColor color)
        {
            Console.SetCursorPosition(p.X, p.Y);
            Console.ForegroundColor = color;
            Console.Write("█");
        }
    }
}