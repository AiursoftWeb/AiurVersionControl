using SnakeGame.Models;
using System;
using System.Collections.Generic;

namespace SnakeGame.Services.Implements
{
    public class Snake : GameObject
    {
        private readonly List<Position> _body = new();
        public Position Head => _body[0];
        // Use for erase tail.
        private Position _lastPosition;
        public int Count => _body.Count;

        public Snake(Position p, int count = 1)
        {
            for (var i = 0; i < count; i++)
            {
                _body.Add(p);
            }
        }

        public void AddBody()
        {
            _body.Add(new Position{ X = _body[^1].X, Y = _body[^1].Y});
        }

        public void Update(Position inputDirection)
        {
            _lastPosition = (Position)_body[^1].Clone();
            
            for (var i = _body.Count - 2; i >= 0; i--)
            {
                _body[i + 1] = (Position)_body[i].Clone();
            }
            Head.X += inputDirection.X;
            Head.Y += inputDirection.Y;
        }

        protected override void DrawObject()
        {
            foreach (Position p in _body)
            {
                Console.SetCursorPosition(p.X, p.Y);
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("█");
            }
            // Erase tail
            if (_lastPosition != null && !_lastPosition.Equals(_body[^1]))
            {
                Console.SetCursorPosition(_lastPosition.X, _lastPosition.Y);
                Console.WriteLine(" ");
            }
        }

        public bool OnSnake(Position p, bool ignoreHead = false)
        {
            if (ignoreHead)
            {
                if (_body.Count <= 4) return false;
                for (var i = 4; i < _body.Count; i++)
                {
                    if (p.Equals(_body[i]))
                    {
                        return true;
                    }
                }

                return false;
            }
            return _body.Contains(p);
        }

        public bool CanEat(Position food)
        {
            return Head.X == food.X && Head.Y == food.Y;
        }

        public bool SnakeIntersection()
        {
            return OnSnake(Head, true);
        }
    }
}
