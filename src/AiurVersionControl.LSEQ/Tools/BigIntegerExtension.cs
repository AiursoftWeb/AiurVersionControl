using System;
using System.Numerics;

namespace AiurVersionControl.LSEQ.Tools
{
    public static class BigIntegerExtension
    {
        public static BigInteger Min(this BigInteger a, BigInteger other)
        {
            return a < other ? a : other;
        }
        
        public static BigInteger Max(this BigInteger a, BigInteger other)
        {
            return a > other ? a : other;
        }

        public static BigInteger RandomBigInteger(long numBits, Random rnd)
        {
            return new (RandomBits(numBits, rnd));
        }

        static byte[] RandomBits(long numBits, Random rnd)
        {
            if (numBits < 0)
                throw new ArgumentException("numBits must be non-negative");
            var numBytes = (numBits+7)/8;
            var randomBits = new byte[numBytes];

            // Generate random bytes and mask out any excess bits
            if (numBytes > 0) {
                rnd.NextBytes(randomBits);
                var excessBits = (int)(8*numBytes - numBits);
                randomBits[0] = (byte) (randomBits[0] & (1 << (8-excessBits)) - 1);
            }
            return randomBits;
        }
    }
}