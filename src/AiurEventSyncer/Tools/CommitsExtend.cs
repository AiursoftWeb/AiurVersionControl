using AiurEventSyncer.Models;
using AiurStore.Abstracts;
using AiurStore.Models;
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
    }
}
