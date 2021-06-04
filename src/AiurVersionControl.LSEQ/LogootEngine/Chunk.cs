using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurVersionControl.LSEQ.LogootEngine
{
    public class Chunk<E>
    {
        public readonly int Position;
        public List<E> Lines;
        public int Size => Lines.Count;

        public Chunk(int position, List<E> lines)
        {
            Position = position;
            Lines = lines;
        }

        public Chunk(int position, E[] lines)
        {
            Position = position;
            Lines = lines.ToList();
        }

        public int HashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + (Lines == null ? 0 : Lines.GetHashCode());
            result = prime * result + Position;
            result = prime * result + Size;
            return result;
        }

        public bool Equals(Chunk<E> other)
        {
            if (this == other)
            {
                return true;
            }
            else if (other == null)
            {
                return false;
            }
            else
            {
                if (Lines == null)
                {
                    if (other.Lines != null)
                    {
                        return false;
                    }
                }
                else if (!Lines.Equals(other.Lines))
                {
                    return false;
                }

                return Position == other.Position;
            }

        }
    }
}
