using AiurStore.Abstracts;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AiurStore.Providers.MemoryProvider
{
    public class MemoryStoreProvider : IStoreProvider
    {
        private readonly ConcurrentQueue<string> _store = new ConcurrentQueue<string>();

        public void Clear()
        {
            _store.Clear();
        }

        public IEnumerable<string> GetAll()
        {
            return _store;
        }

        public void Insert(string newItem)
        {
            _store.Enqueue(newItem);
        }
    }
}
