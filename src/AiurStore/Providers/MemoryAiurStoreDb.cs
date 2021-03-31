using AiurStore.Models;
using AiurStore.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace AiurStore.Providers
{
    public class MemoryAiurStoreDb<T> : InOutDatabase<T>
    {
        private readonly LinkedList<T> _store = new();
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        private LinkedListNode<T> SearchFromLast(Predicate<T> prefix)
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
            throw new InvalidOperationException("Result not found.");
        }

        public override IEnumerable<T> GetAll()
        {
            return _store;
        }

        public override IEnumerable<T> GetAllAfter(Predicate<T> prefix)
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

            var start = _store.FindLast(afterWhich);
            return ListExtends.YieldAfter(start);
        }

        public override void Add(T newItem)
        {
            _store.AddLast(newItem);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem));
        }

        public override void InsertAfter(T afterWhich, T newItem)
        {
            if (afterWhich == null)
            {
                _store.AddFirst(newItem);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem, 0));
            }
            else
            {
                var which = _store.FindLast(afterWhich);
                if (which == null) throw new KeyNotFoundException($"Insertion point {nameof(afterWhich)} not found.");
                _store.AddAfter(which, newItem);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }
}
