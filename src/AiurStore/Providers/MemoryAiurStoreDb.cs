using AiurStore.Models;
using System;
using System.Collections.Generic;

namespace AiurStore.Providers
{
    public class MemoryAiurStoreDb<T> : InOutDatabase<T> where T : class
    {
        private readonly LinkedList<T> _store = new LinkedList<T>();

        private LinkedListNode<T> SearchFromLast(Func<T, bool> prefix)
        {
            var start = _store.Last;
            while (start != null)
            {
                if (prefix(start.Value))
                {
                    return start;
                }
                start = start.Previous;
            }
            return null;
        }

        public override IEnumerable<T> GetAll()
        {
            return _store;
        }

        public override IEnumerable<T> GetAllAfter(Func<T, bool> prefix)
        {
            var element = SearchFromLast(prefix);
            return GetAllAfter(element?.Value);
        }

        public override IEnumerable<T> GetAllAfter(T afterWhich)
        {
            if (afterWhich == null)
            {
                foreach (var item in _store)
                {
                    yield return item;
                }
            }
            else
            {
                var start = _store.FindLast(afterWhich)?.Next;
                while (start != null)
                {
                    yield return start.Value;
                    start = start.Next;
                }
            }
        }

        public override void Add(T newItem)
        {
            _store.AddLast(newItem);
        }

        public override void Clear()
        {
            _store.Clear();
        }

        public override void InsertAfter(T afterWhich, T newItem)
        {
            if (afterWhich == null)
            {
                _store.AddFirst(newItem);
            }
            else
            {
                var which = _store.FindLast(afterWhich);
                _store.AddAfter(which, newItem);
            }
        }
    }
}
