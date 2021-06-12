using AiurVersionControl.LSEQ.BaseComponents;
using AiurVersionControl.LSEQ.Data;
using AiurVersionControl.LSEQ.LogootEngine;
using AiurVersionControl.LSEQ.StrategiesComponents;
using AiurVersionControl.LSEQ.StrategiesComponents.Boundary;
using AiurVersionControl.LSEQ.StrategyChoiceComponent;
using NetDiff;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace AiurVersionControl.LSEQ
{
    public class Program
    {
        public static void Main()
        {
            // Do a doc simulator soon
            var b = new BaseDouble(5);
            var boundary = new ConstantBoundary(new BigInteger(10));
            var choise = new RandomStrategyChoice(b, new BeginningBoundaryIdProvider(b, boundary), new EndingBoundaryIdProvider(b, boundary));
            var logootEngine = new LogootEngine.LogootEngine(b, choise);
            logootEngine.Replica = new Replica();

            for (int i = 0; i < 10000; ++i)
            {
                MyPatch patch = logootEngine.GeneratePatch(getDeltas(1, i));
                logootEngine.Deliver(patch);
            }

            var idtable = logootEngine.IdTable;

            idtable.ForEach(e => Console.WriteLine(e));
        }

        public static DiffResult<Chunk<string>>[] getDeltas(int N, int M)
        {

            List<DiffResult<Chunk<string>>> deltas = new List<DiffResult<Chunk<string>>>();

            /* Create chunk of inserted line */
            List<string> insertContent = new List<string>();
            for (int i = 0; i < N; ++i)
            {
                //insertContent.add("" + i + " " + DocumentSimulator.nbLine);
                insertContent.Add("" + M);
            }
            Chunk<string> insertChunk = new Chunk<string>(0, insertContent); // at beginning

            List<string> insertOriginal = new List<string>();
            Chunk<string> originalChunk = new Chunk<string>(0, insertOriginal);

            DiffResult<Chunk<string>> d = new DiffResult<Chunk<string>>(originalChunk, insertChunk, DiffStatus.Inserted);

            deltas.Add(d);
            return deltas.ToArray();
        }
    }
}