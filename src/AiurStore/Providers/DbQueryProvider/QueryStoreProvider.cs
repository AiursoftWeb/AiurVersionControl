using AiurStore.Abstracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AiurStore.Providers.DbQueryProvider
{
    public class QueryStoreProvider : IStoreProvider
    {
        private Func<IQueryable<string>> _queryFactory;
        private Action<string> _add;

        public QueryStoreProvider(
            Func<IQueryable<string>> query,
            Action<string> add)
        {
            _queryFactory = query;
            _add = add;
        }

        public IEnumerable<string> GetAll()
        {
            return _queryFactory();
        }

        public void Add(string newItem)
        {
            _add?.Invoke(newItem);
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
