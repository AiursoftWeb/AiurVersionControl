using System;
using System.Collections.Generic;
using System.Numerics;
using AiurVersionControl.LSEQ.BaseComponents;
using AiurVersionControl.LSEQ.Data;
using AiurVersionControl.LSEQ.StrategyChoiceComponent;
using NetDiff;

namespace AiurVersionControl.LSEQ.LogootEngine
{
    public class LogootEngine : ILogootEngine
    {
        public List<Positions> IdTable { get; set; }

        public List<string> Doc { get; set; }

        public IBase Base { get; set; }

        public Replica Replica { get; set; }

        public IStrategyChoice StrategyChoice { get; set; }

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

        public void Deliver(MyPatch patch)
        {
            bool one_insert = false;

            foreach (MyDelta delta in patch)
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

        public MyPatch GeneratePatch(DiffResult<Chunk<string>>[] deltas)
        {
            MyPatch patch = new();

            for (int j = 0; j < deltas.Length; ++j)
            {
                // foreach delta
                IEnumerator<Positions> ids;
                switch (deltas[j].Status)
                {
                    case DiffStatus.Inserted:
                        ids = insert(deltas[j]);
                        ids.MoveNext();
                        for (int k = 0; k < deltas[j].Obj2.Size; ++k)
                        {
                            // foreach line inserted
                            MyDelta md = new MyDelta(Operation.Insert, ids.Current,
                                    deltas[j].Obj2.Lines[k]
                                            .ToString());
                            patch.Add(md);
                        }
                        break;
                    case DiffStatus.Modified:
                        // foreach line changed (<=> delete & insert )
                        for (int k = 0; k < deltas[j].Obj1.Size; ++k)
                        { // deleted lines
                            MyDelta md = new MyDelta(Operation.Delete,
                                    IdTable[deltas[j].Obj1.Position
                                            + k + 1], "");
                            patch.Add(md);
                        }

                        ids = insert(deltas[j]);
                        ids.MoveNext();
                        for (int k = 0; k < deltas[j].Obj2.Size; ++k)
                        { // inserted line
                            MyDelta md = new MyDelta(Operation.Insert, ids.Current,
                                    deltas[j].Obj2.Lines[k]
                                            .ToString());
                            patch.Add(md);
                        }
                        break;
                    case DiffStatus.Deleted:
                        for (int k = 0; k < deltas[j].Obj1.Size; ++k)
                        {
                            MyDelta md = new MyDelta(Operation.Delete,
                                    IdTable[deltas[j].Obj1.Position
                                            + k + 1], "");
                            patch.Add(md);
                        }
                        break;
                    default: // NOTHING
                        break;
                }

            }
            return patch;
        }

        private IEnumerator<Positions> insert(DiffResult<Chunk<string>> delta)
        {
            Positions previous = IdTable[delta.Obj1.Position];
            Positions next = IdTable[delta.Obj1.Position
                    + delta.Obj1.Size + 1];

            return StrategyChoice.GenerateIdentifiers(previous, next, delta
                    .Obj2.Size, Replica);
        }
    }
}