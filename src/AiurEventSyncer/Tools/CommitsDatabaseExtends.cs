using AiurEventSyncer.Abstract;
using AiurStore.Models;
using System.Collections.Generic;

namespace AiurEventSyncer.Tools
{
    public static class CommitsDatabaseExtends
    {
        public static IEnumerable<T> GetCommitsAfterId<T, D>(this IOutOnlyDatabase<T> database, string id) where T : Commit<D>
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return database.GetAll();
            }
            return database.GetAllAfter(t => t.Id == id);
        }
    }
}
