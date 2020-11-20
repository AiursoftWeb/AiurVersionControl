using AiurStore.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AiurStore.Providers.DbQueryProvider
{
    public class QueryStoreProvider<DbContext> : IStoreProvider
    {
        private readonly Func<DbContext> _contextFactory;
        private Func<DbContext, IQueryable<string>> _queryFactory;
        private readonly Action<string, DbContext> _addAction;

        public QueryStoreProvider(
            Func<DbContext> contextFactory,
            Func<DbContext, IQueryable<string>> queryFactory,
            Action<string, DbContext> addAction)
        {
            _contextFactory = contextFactory;
            _queryFactory = queryFactory;
            _addAction = addAction;
        }

        public IEnumerable<string> GetAll()
        {
            var context = _contextFactory();
            return _queryFactory(context);
        }

        public void Add(string newItem)
        {
            var context = _contextFactory();
            _addAction(newItem, context);
        }

        public void Clear()
        {
            throw new NotImplementedException("You can't clear if you are using QueryStoreProvider!");
        }

        public void Insert(int index, string newItem)
        {
            throw new NotImplementedException("You can't insert if you are using QueryStoreProvider!");
        }
    }
}
