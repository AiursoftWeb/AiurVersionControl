using System.Collections.Generic;
using System.Numerics;
using AiurVersionControl.LSEQ.Data;
using AiurVersionControl.LSEQ.LogootEngine;

namespace AiurVersionControl.LSEQ.StrategiesComponents
{
    public interface IIdProviderStrategy
    {
        /// <summary>
        /// Generate N identifier between p & q
        /// </summary>
        /// <param name="p">previous identifier</param>
        /// <param name="q">next identifier</param>
        /// <param name="n">number of line inserted</param>
        /// <param name="rep">replica information to store</param>
        /// <param name="interval"></param>
        /// <param name="index"></param>
        /// <returns>list of unique identifiers which can be used in logoot</returns>
        public IEnumerable<Positions> GenerateIdentifiers(Positions p, Positions q,
            int n, Replica rep, BigInteger interval, int index);
    }
}