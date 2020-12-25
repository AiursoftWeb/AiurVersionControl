using SnakeGame.Models;
using System;

namespace SnakeGame.Services
{
    public abstract class GameObject : IDrawable
    {
        public void Draw()
        {
            DrawObject();
        }

        protected abstract void DrawObject();
        
        public static void Draw(Position p, ConsoleColor color)
        {
            Console.SetCursorPosition(p.X, p.Y);
            Console.ForegroundColor = color;
            Console.Write("█");
        }
    }
}