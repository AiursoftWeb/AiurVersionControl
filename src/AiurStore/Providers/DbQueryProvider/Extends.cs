using AiurStore.Models;
using System;
using System.Linq;

namespace AiurStore.Providers.DbQueryProvider
{
    public static class DbQueryProviderTools
    {
        public static void UseQueryStore<D, T>(this InOutDbOptions<T> options,
            Func<D> contextFactory,
            Func<D, IQueryable<T>> queryFactory,
            Action<T, D> addAction)
        {
            options.Provider = new QueryStoreProvider<D, T>(contextFactory, queryFactory, addAction);
        }

        public static InOutDatabase<T> BuildFromDbSet<T, D>(
            Func<D> contextFactory,
            Func<D, IQueryable<T>> queryFactory,
            Action<T, D> addAction)
        {
            return new DummyQueryDb<T>
            {
                Provider = new QueryStoreProvider<D, T>(contextFactory, queryFactory, addAction)
            };
        }
    }
}
