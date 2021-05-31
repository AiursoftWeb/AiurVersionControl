using System.Collections.Generic;
using AiurVersionControl.LSEQ.Data;
using AiurVersionControl.LSEQ.LogootEngine;

namespace AiurVersionControl.LSEQ.StrategyChoiceComponent
{
    public interface IStrategyChoice
    {
        /// <summary>
        /// Function which will defer the creation of identifiers to a bunch of
        /// IdProviders
        /// </summary>
        /// <param name="p">previous identifier</param>
        /// <param name="q">next identifier</param>
        /// <param name="n">number of line inserted</param>
        /// <param name="rep">replica information to store</param>
        /// <returns></returns>
        public IEnumerator<Positions> GenerateIdentifiers(Positions p,
            Positions q, int n, Replica rep);
        
        /// <summary>
        /// Add data to the strategy choice
        /// </summary>
        /// <param name="p">previous Id</param>
        /// <param name="id">inserted</param>
        /// <param name="q">next Id</param>
        public void Add(Positions prev, Positions id, Positions next);
        
        public void Del(Positions id);
        
        public void IncDate();
        
        public Dictionary<Positions, FakeListNode> GetSpectrum();
    }
}