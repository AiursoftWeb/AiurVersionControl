using AiurStore.Abstracts;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AiurStore.Providers.MemoryProvider
{
    public class MemoryStoreProvider : IStoreProvider
    {
        private readonly List<string> _store = new List<string>();

        public void Clear()
        {
            _store.Clear();
        }

        public IEnumerable<string> GetAll()
        {
            return _store;
        }

        public void Add(string newItem)
        {
            _store.Add(newItem);
        }

        public void Insert(int index, string newItem)
        {
            _store.Insert(index, newItem);
        }
    }
}
