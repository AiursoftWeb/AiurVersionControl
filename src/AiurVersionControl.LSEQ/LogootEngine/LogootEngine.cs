using System.Collections.Generic;
using AiurVersionControl.LSEQ.BaseComponents;
using AiurVersionControl.LSEQ.Data;

namespace AiurVersionControl.LSEQ.LogootEngine
{
    public class LogootEngine : ILogootEngine
    {
        private List<Positions> _idTable;
        private List<string> _doc;

        private IBase _base;
        
        private Replica _replica;
        
        private IStrategyChoice _strategyChoice;
        
        
        public void Deliver(Patch patch)
        {
            throw new System.NotImplementedException();
        }

        public Patch GeneratePatch(List<Delta> deltas)
        {
            throw new System.NotImplementedException();
        }
    }
}