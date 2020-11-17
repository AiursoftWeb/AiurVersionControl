using AiurEventSyncer.Models;
using System.Collections.Generic;

namespace AiurEventSyncer.Tools
{
    public static class CommitsExtend
    {
        public static IEnumerable<Commit<T>> AfterCommitId<T>(this IEnumerable<Commit<T>> source, string sourcePointerPosition)
        {
            if (string.IsNullOrWhiteSpace(sourcePointerPosition))
            {
                return source;
            }
            return source.After(t => t.Id == sourcePointerPosition);
        }
    }
}
