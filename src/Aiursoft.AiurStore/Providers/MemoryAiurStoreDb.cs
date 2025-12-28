using System.Collections.Specialized;
using Aiursoft.AiurStore.Models;
using Aiursoft.AiurStore.Tools;

namespace Aiursoft.AiurStore.Providers
{
    public class MemoryAiurStoreDb<T> : InOutDatabase<T>
    {
        private readonly LinkedList<T> _store = new();
        private readonly object _lock = new();
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        private LinkedListNode<T> SearchFromLast(Predicate<T> prefix)
        {
            lock (_lock)
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
            }
            throw new InvalidOperationException("Result not found.");
        }

        public override IEnumerable<T> GetAll()
        {
            lock (_lock)
            {
                return _store.ToList();
            }
        }

        public override IEnumerable<T> GetAllAfter(Predicate<T> prefix)
        {
            lock (_lock)
            {
                var node = SearchFromLast(prefix);
                return ListExtends.YieldAfter(node).ToList();
            }
        }

        public override IEnumerable<T> GetAllAfter(T afterWhich)
        {
            lock (_lock)
            {
                if (afterWhich == null)
                {
                    return _store.ToList();
                }

                var start = _store.FindLast(afterWhich);
                return ListExtends.YieldAfter(start).ToList();
            }
        }

        public override void Add(T newItem)
        {
            lock (_lock)
            {
                _store.AddLast(newItem);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem));
        }

        public override void InsertAfter(T afterWhich, T newItem)
        {
            lock (_lock)
            {
                if (afterWhich == null)
                {
                    _store.AddFirst(newItem);
                }
                else
                {
                    var which = _store.FindLast(afterWhich);
                    if (which == null) throw new KeyNotFoundException($"Insertion point {nameof(afterWhich)} not found.");
                    _store.AddAfter(which, newItem);
                }
            }

            if (afterWhich == null)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem, 0));
            }
            else
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public override int Count
        {
            get
            {
                lock (_lock)
                {
                    return _store.Count;
                }
            }
        }
    }
}
