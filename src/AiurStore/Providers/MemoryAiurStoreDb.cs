using AiurStore.Models;
using System.Collections.Generic;

namespace AiurStore.Providers
{
    public class MemoryAiurStoreDb<T> : InOutDatabase<T>
    {
#warning Use linked list for better performance.
        private readonly List<T> _store = new List<T>();

        public override IEnumerable<T> GetAll()
        {
            return _store;
        }

        public override void Add(T newItem)
        {
            _store.Add(newItem);
        }

        public override void Clear()
        {
            _store.Clear();
        }

        public override void Insert(int index, T newItem)
        {
            _store.Insert(index, newItem);
        }
    }
}
