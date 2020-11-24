using AiurStore.Models;
using System;
using System.Linq;

namespace AiurStore.Providers.DbQueryProvider
{
    public static class DbQueryProviderTools
    {
        public static void UseQueryStore<D>(this InOutDbOptions options,
            Func<D> contextFactory,
            Func<D, IQueryable<string>> queryFactory,
            Action<string, D> addAction)
        {
            options.Provider = new QueryStoreProvider<D>(contextFactory, queryFactory, addAction);
        }

        public static InOutDatabase<T> BuildFromDbSet<T, D>(
            Func<D> contextFactory,
            Func<D, IQueryable<string>> queryFactory,
            Action<string, D> addAction)
        {
            return new DummyQueryDb<T>
            {
                Provider = new QueryStoreProvider<D>(contextFactory, queryFactory, addAction)
            };
        }
    }
}
