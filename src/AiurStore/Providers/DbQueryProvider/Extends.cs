using AiurStore.Models;
using AiurStore.Providers.FileProvider;
using System;
using System.Linq;

namespace AiurStore.Providers.DbQueryProvider
{
    public static class Extends
    {
        public static void UseQueryStore(this InOutDbOptions options, IQueryable<string> query, Action<string> addAction)
        {
            options.Provider = new QueryStoreProvider(query, addAction);
        }
    }
}
