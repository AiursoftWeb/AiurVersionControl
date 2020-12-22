using System;
using SnakeGame.Models;

namespace SnakeGame.Launcher
{
    public static class Input
    {
        public static void ChangeDirection(ConsoleKey command, Position p)
        {
            switch (command)
            {
                case ConsoleKey.LeftArrow:
                    if (p.X != 0) break;
                    p.X = -1;
                    p.Y = 0;
                    break;
                case ConsoleKey.UpArrow:
                    if (p.Y != 0) break;
                    p.X = 0;
                    p.Y = -1;
                    break;
                case ConsoleKey.RightArrow:
                    if (p.X != 0) break;
                    p.X = 1;
                    p.Y = 0;
                    break;
                case ConsoleKey.DownArrow:
                    if (p.Y != 0) break;
                    p.X = 0;
                    p.Y = 1;
                    break;
            }
        }
    }
}
