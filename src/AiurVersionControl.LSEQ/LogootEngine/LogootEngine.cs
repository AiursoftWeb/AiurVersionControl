using System;
using System.Collections.Generic;
using System.Numerics;
using AiurVersionControl.LSEQ.BaseComponents;
using AiurVersionControl.LSEQ.Data;
using AiurVersionControl.LSEQ.StrategyChoiceComponent;

namespace AiurVersionControl.LSEQ.LogootEngine
{
    public class LogootEngine : ILogootEngine
    {
        private List<Positions> IdTable { get; set; }
        private List<string> Doc { get; set; }

        private IBase Base { get; set; }

        private Replica Replica { get; set; }

        private IStrategyChoice StrategyChoice { get; set; }

        /**
	     * Initialize Logoot data & document
	     */
        public LogootEngine(IBase b, IStrategyChoice strategyChoice)
        {
            Base = b;
            StrategyChoice = strategyChoice;

            var fakeReplica = new Replica
            {
                Clock = -666,
                Id = -666
            };

            var first = new Positions(BigInteger.Zero, b.GetBaseBase(),
                fakeReplica);

            fakeReplica.Clock = 666;
            fakeReplica.Id = 666;

            var last = new Positions(BigInteger
               .Pow(new BigInteger(2), Base.GetBaseBase()) - BigInteger.One,
               b.GetBaseBase(), fakeReplica);

            IdTable = new List<Positions>
            {
                first,
                last
            };

            Doc = new List<string>();
        }

        public void Deliver(Patch patch)
        {
            bool one_insert = false;

            foreach (Delta delta in patch)
            {
                int position;

                switch (delta.Type)
                {
                    case Operation.Insert:
                        one_insert = true;
                        position = - IdTable.BinarySearch(delta.Id) - 1;
                        Doc.Insert(position - 1, delta.Content);
                        IdTable.Insert(position, delta.Id);
                        StrategyChoice.Add(IdTable[position - 1],
                                IdTable[position], IdTable[position + 1]);
                        break;

                    case Operation.Delete:

                        position = IdTable.BinarySearch(delta.Id);
                        if (position > 0)
                        {
                            Doc.RemoveAt(position - 1);
                            IdTable.RemoveAt((int)position);
                            StrategyChoice.Del(delta.Id);
                        }
                        else
                        {
                            // Console.WriteLine("MIAOU MIAOU");
                        }
                        break;
                    default: // NOTHING
                        break;
                }

            }
            if (one_insert)
            { // At least of insert
                StrategyChoice.IncDate();
            }
        }

        public Patch GeneratePatch(DiffResult<string>[] deltas)
        {
            Patch patch = new();

            for (int j = 0; j < deltas.Count; ++j)
            {
                // foreach delta

                IEnumerable<Positions> ids;
                switch (deltas[j].Type)
                {
                    case Operation.Insert:
                        // Need to convert DiffResult to deltas tomorrow!

                        break;
                    //case Operation.Change:
                        //break;
                    case Operation.Delete:
                        // TODO
                        break;
                    default: // NOTHING
                        break;
                }

            }
            return patch;
        }

        private IEnumerable<Positions> insert(Delta delta)
        {
            throw new NotImplementedException();
        }
    }
}