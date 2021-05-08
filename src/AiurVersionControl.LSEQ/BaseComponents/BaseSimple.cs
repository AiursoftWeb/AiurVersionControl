using System.Numerics;

namespace AiurVersionControl.LSEQ.BaseComponents
{
    public class BaseSimple : IBase
    {
        private readonly int baseBase;

        public BaseSimple(int baseBase)
        {
            this.baseBase = baseBase;
        }
        
        public int GetSumBit(int depth)
        {
            return baseBase * depth;
        }

        public int GetBitBase(int depth)
        {
            return baseBase;
        }

        public int GetBaseBase()
        {
            return baseBase;
        }

        public BigInteger Interval(BigInteger p, BigInteger q, int index)
        {
            var prevBitLength = p.GetBitLength();
            var nextBitLength = p.GetBitLength();

            var bitBaseSum = GetSumBit(index);
            var result = BigInteger.Zero;
            
            // #1 truncate or add
            // #1a: on previous digit
            BigInteger prev;
            if (prevBitLength < bitBaseSum)
            {
                // Add 0 and +1 to result
                prev = p << (int) (bitBaseSum - prevBitLength);
            }
            else
            {
                prev = p >> (int) (prevBitLength - bitBaseSum);
            }
            
            // #1b: on next digit
            BigInteger next;
            if (nextBitLength < bitBaseSum)
            {
                // Add 1 and +1 to result
                next = q << (int) (bitBaseSum - nextBitLength);
            }
            else
            {
                next = q >> (int) (nextBitLength - bitBaseSum);
            }

            result += (next - prev - BigInteger.One);
            
            return result;
        }
    }
}