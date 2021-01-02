using AiurEventSyncer.Abstract;
using AiurStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
