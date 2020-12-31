using AiurStore.Abstracts;
using System.Collections.Generic;

namespace AiurStore.Providers.MemoryProvider
{
    public class MemoryStoreProvider<T> : IStoreProvider<T>
    {
#warning Use linked list for better performance.
        private readonly List<T> _store = new List<T>();

        public void Clear()
        {
            _store.Clear();
        }

        public IEnumerable<T> GetAll()
        {
            return _store;
        }

        public void Add(T newItem)
        {
            _store.Add(newItem);
        }

        public void Insert(int index, T newItem)
        {
            _store.Insert(index, newItem);
        }
    }
}
