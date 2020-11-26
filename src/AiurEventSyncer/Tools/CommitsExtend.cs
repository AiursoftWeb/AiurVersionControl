using AiurEventSyncer.Models;
using AiurStore.Abstracts;
using AiurStore.Models;
using System;
using System.Collections.Generic;

namespace AiurEventSyncer.Tools
{
    public static class CommitsExtend
    {
        public static IEnumerable<Commit<T>> AfterCommitId<T>(this IAfterable<Commit<T>> source, string sourcePointerPosition)
        {
            if (string.IsNullOrWhiteSpace(sourcePointerPosition))
            {
                return source;
            }
            return source.After(t => t.Id == sourcePointerPosition);
        }

        public static void InsertAfterCommitId<T>(this InOutDatabase<Commit<T>> source, string sourcePointerPosition, Commit<T> newCommit)
        {
            if (string.IsNullOrWhiteSpace(sourcePointerPosition))
            {
                source.Insert(0, newCommit);
            }
            else
            {
                source.InsertAfter(t => t.Id == sourcePointerPosition, newCommit);
            }
        }

        private static readonly long x = 131;
        private static readonly long y = (long)(1e9 + 7);

        public static int SharedRange(string[] endWithShared, string[] startWithShared)
        {
            if (endWithShared.Length == 0 || startWithShared.Length == 0)
            {
                return 0;
            }
            long a = 0, b = 0, c = 1;
            int j = endWithShared.Length - 1;

            int pos = 0;

            for (int i = 0; i < startWithShared.Length; i++)
            {
                if (j < 0) break;

                a = ((a * x % y) + (Math.Abs(startWithShared[i].GetHashCode()) % y)) % y;
                b = (b + c * (Math.Abs(endWithShared[j].GetHashCode()) % y) % y) % y;

                if (a == b)
                {
                    pos = i + 1;
                }

                c = c * x % y;
                j--;

            }

            return pos;
        }
    }
}
