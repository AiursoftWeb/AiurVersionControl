using AiurEventSyncer.Abstract;
using AiurStore.Models;
using System.Collections.Generic;

namespace AiurEventSyncer.Tools
{
    public static class CommitsDatabaseExtends
    {
        public static IEnumerable<T> GetCommitsAfterId<T>(this InOutDatabase<T> database, string id) where T : class, ICommit
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return database.GetAll();
            }
            return database.GetAllAfter(t => t.Id == id);
        }
    }
}
