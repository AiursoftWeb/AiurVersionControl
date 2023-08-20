using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Aiursoft.AiurStore.Models
{
    /// <summary>
    /// Describe a collection which can be queried after a statement and inserted after an item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class InOutDatabase<T> : IOutOnlyDatabase<T>, INotifyCollectionChanged
    {
        public abstract event NotifyCollectionChangedEventHandler CollectionChanged;
        public abstract IEnumerable<T> GetAll();
        public abstract IEnumerable<T> GetAllAfter(T afterWhich);
        public abstract IEnumerable<T> GetAllAfter(Predicate<T> prefix);
        public abstract void Add(T newItem);
        public abstract void InsertAfter(T afterWhich, T newItem);
        public abstract int Count { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}
