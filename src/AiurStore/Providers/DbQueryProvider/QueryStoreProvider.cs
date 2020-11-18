using AiurStore.Abstracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AiurStore.Providers.DbQueryProvider
{
    public class QueryStoreProvider : IStoreProvider
    {
        private IQueryable<string> _query;
        private Action<string> _add;

        public QueryStoreProvider(
            IQueryable<string> query,
            Action<string> add)
        {
            _query = query;
            _add = add;
        }

        public IEnumerable<string> GetAll()
        {
            return _query;
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
