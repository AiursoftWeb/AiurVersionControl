using AiurStore.Models;
using AiurStore.Providers.FileProvider;
using System;
using System.Linq;

namespace AiurStore.Providers.DbQueryProvider
{
    public static class DbQueryProviderTools
    {
        public static void UseQueryStore(this InOutDbOptions options, Func<IQueryable<string>> queryFactory, Action<string> addAction)
        {
            options.Provider = new QueryStoreProvider(queryFactory, addAction);
        }

        public static InOutDatabase<T> BuildFromDbSet<T>(Func<IQueryable<string>> queryFactory, Action<string> addAction)
        {
            var db = new DummyQueryDb<T>();
            db.Provider = new QueryStoreProvider(queryFactory, addAction);
            return db;
        }
    }
}
