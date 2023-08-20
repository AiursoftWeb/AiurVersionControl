using Aiursoft.AiurEventSyncer.Abstract;
using Aiursoft.AiurStore.Models;
using System.Collections.Generic;

namespace Aiursoft.AiurEventSyncer.Tools
{
    public static class CommitsDatabaseExtends
    {
        public static IEnumerable<T> GetCommitsAfterId<T, TD>(this IOutOnlyDatabase<T> database, string id) where T : Commit<TD>
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return database.GetAll();
            }
            return database.GetAllAfter(t => t.Id == id);
        }
    }
}
