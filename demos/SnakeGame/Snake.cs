using System;
using System.Collections.Generic;
using SnakeGame.Models;

namespace SnakeGame
{
    public class Snake
    {
        readonly List<Position> _body = new List<Position>();
        public Position Head => _body[0];
        // Use for erase tail.
        private readonly Position _lastPosition = new Position(0, 0);

        public Snake(Position p)
        {
            this._body.Add(p);
            DrawSnake();
        }

        public void AddBody()
        {
            this._body.Add(new Position(this._body[^1].X, this._body[^1].Y));
        }

        public void Update(Position inputDirection)
        {
            _lastPosition.X = this._body[^1].X;
            _lastPosition.Y = this._body[^1].Y;
            
            for (int i = this._body.Count - 2; i >= 0; i--)
            {
                this._body[i + 1].X = this._body[i].X;
                this._body[i + 1].Y = this._body[i].Y;
            }
            this._body[0].X += inputDirection.X;
            this._body[0].Y += inputDirection.Y;
            DrawSnake();
        }

        private void DrawSnake()
        {
            foreach (Position p in this._body)
            {
                Console.SetCursorPosition(p.X, p.Y);
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("█");
            }
            // Erase tail
            if (_lastPosition != null && !_lastPosition.Equals(this._body[^1]))
            {
                Console.SetCursorPosition(_lastPosition.X, _lastPosition.Y);
                Console.WriteLine(" ");
            }
        }

        public bool OnSnake(Position p, bool ignoreHead = false)
        {
            if (ignoreHead == true)
            {
                if (this._body.Count <= 4) return false;
                for (int i = 4; i < this._body.Count; i++)
                {
                    if (p.Equals(this._body[i]))
                    {
                        return true;
                    }
                }

                return false;
            }
            return this._body.Contains(p);
        }

        public bool CanEat(Position food)
        {
            return this.Head.X == food.X && this.Head.Y == food.Y;
        }

        public bool SnakeIntersection()
        {
            return OnSnake(this.Head, true);
        }
    }
}
