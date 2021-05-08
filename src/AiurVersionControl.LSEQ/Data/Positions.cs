using System;
using System.Numerics;
using AiurVersionControl.LSEQ.LogootEngine;

namespace AiurVersionControl.LSEQ.Data
{
    public class Positions : IComparable<Positions>
    {
        public readonly BigInteger D;
        public readonly int S;
        public readonly int C;

        public Positions(BigInteger r, int bitSize, Replica rep)
        {
            D = r | (1<<bitSize); // set the departure bit to 1. Thus the 0 in
                                    // front won't be automatically truncated by
                                    // BigInteger
            S = rep.Id;
            C = rep.Clock;
        }
        
        public int CompareTo(Positions o)
        {
            // #1 truncate
            var myBitLength = D.GetBitLength();
            var otBitLength = o.D.GetBitLength();
            
            var difBitLength = (int) (myBitLength - otBitLength);
            
            BigInteger other;
            BigInteger mine;

            if (difBitLength > 0)
            {
                other = o.D;
                mine = D >> difBitLength;
            }
            else
            {
                other = o.D >> difBitLength;
                mine = D;
            }
            
            // #2 compare digit
            int comp = mine.CompareTo(other);
            if (comp != 0)
            {
                return comp;
            }
            
            // #3 compare s and c
            comp = S.CompareTo(o.S);
            if (comp != 0)
            { // s != o.s
                return comp;
            }
            else
            {
                comp = C.CompareTo(o.C);
                if (comp != 0)
                { // C != o.C
                    return comp;
                }
            }
            
            // #4 compare size
            if (myBitLength > otBitLength)
            {
                return 1;
            }
            else if (myBitLength < otBitLength)
            {
                return -1;
            }
            
            return 0;
        }
    }
}