using AiurStore.Models;
using AiurStore.Tools;
using System;
using System.Collections.Generic;

namespace AiurStore.Providers
{
    public class MemoryAiurStoreDb<T> : InOutDatabase<T>
    {
        private readonly LinkedList<T> _store = new LinkedList<T>();

        private LinkedListNode<T> SearchFromLast(Func<T, bool> prefix)
        {
            var last = _store.Last;
            while (last != null)
            {
                if (prefix(last.Value))
                {
                    return last;
                }
                last = last.Previous;
            }
            throw new InvalidOperationException("Result no found.");
        }

        public override IEnumerable<T> GetAll()
        {
            return _store;
        }

        public override IEnumerable<T> GetAllAfter(Func<T, bool> prefix)
        {
            var node = SearchFromLast(prefix);
            return ListExtends.YieldAfter(node);
        }

        public override IEnumerable<T> GetAllAfter(T afterWhich)
        {
            if (afterWhich == null)
            {
                return _store;
            }
            else
            {
                var start = _store.FindLast(afterWhich);
                return ListExtends.YieldAfter(start);
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
