using System;
using System.Collections.Generic;
using System.Numerics;
using AiurVersionControl.LSEQ.BaseComponents;
using AiurVersionControl.LSEQ.Data;
using AiurVersionControl.LSEQ.LogootEngine;
using AiurVersionControl.LSEQ.StrategiesComponents.Boundary;
using AiurVersionControl.LSEQ.Tools;

namespace AiurVersionControl.LSEQ.StrategiesComponents
{
    public class EndingBoundaryIdProvider : IIdProviderStrategy
    {
        private readonly Random _rand = new Random();
        
        private readonly IBase _base;
        
        private readonly IBoundary _boundary;
        
        public EndingBoundaryIdProvider(IBase b, IBoundary boundary)
        {
            _base = b;
            _boundary = boundary;
        }
        
        public IEnumerator<Positions> GenerateIdentifiers(Positions p, Positions q, int n, Replica rep, BigInteger interval, int index)
        {
            var positions = new List<Positions>();
            
            // #0 process the interval for random
            var step = interval / new BigInteger(n);
            step = step.Min(_boundary.GetBoundary(index)).Max(BigInteger.One);
            
            // #1 Truncate tail or add bits
            var prevBitCount = p.D.GetBitLength() - 1;
            var diffBitCount = (int) (prevBitCount - _base.GetSumBit(index));

            var r = p.D >> diffBitCount;
            
            // #2 create position by adding a random value; N times
            for (int j = 0; j < n; ++j)
            {
                BigInteger randomInt;
                
                // Random
                if (step <= 1)
                {
                    randomInt = BigInteger.One;
                }
                else
                {
                    do
                    {
                        randomInt = BigIntegerExtension.RandomBigInteger((step - BigInteger.One).GetBitLength(), _rand);
                    }
                    while (randomInt >= (step - BigInteger.One));
                    randomInt += BigInteger.One;
                }
                // // Construct
                BigInteger newR = r - randomInt;
                rep.Clock += 1;
                Positions tempPositions = new Positions(newR,
                    _base.GetSumBit(index), rep);
                positions.Add(tempPositions);
                r -= step;
            }

            positions.Reverse();
            return positions.GetEnumerator();
        }
    }
}