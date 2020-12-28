using AiurStore.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AiurStore.Providers.DbQueryProvider
{
    public class QueryStoreProvider<DbContext, T> : IStoreProvider<T>
    {
        private readonly Func<DbContext> _contextFactory;
        private readonly Func<DbContext, IQueryable<T>> _queryFactory;
        private readonly Action<T, DbContext> _addAction;

        public QueryStoreProvider(
            Func<DbContext> contextFactory,
            Func<DbContext, IQueryable<T>> queryFactory,
            Action<T, DbContext> addAction)
        {
            _contextFactory = contextFactory;
            _queryFactory = queryFactory;
            _addAction = addAction;
        }

        public IEnumerable<T> GetAll()
        {
            var context = _contextFactory();
            return _queryFactory(context);
        }

        public void Add(T newItem)
        {
            var context = _contextFactory();
            _addAction(newItem, context);
        }

        public void Clear()
        {
            throw new NotImplementedException("You can't clear if you are using QueryStoreProvider!");
        }

        public void Insert(int index, T newItem)
        {
            throw new NotImplementedException("You can't insert if you are using QueryStoreProvider!");
        }
    }
}
