using System;
using System.Collections.Generic;
using System.Text;

namespace SnakeGame.Models
{
    public class Position
    {
        public int X;
        public int Y;

        public Position(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Position p)
            {
                return this.X == p.X && this.Y == p.Y;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() + this.Y.GetHashCode();
        }
        
        public override string ToString()
        {
            return X + "," + Y;
        }
    }
}
