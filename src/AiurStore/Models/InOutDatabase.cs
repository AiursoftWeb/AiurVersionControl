using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace AiurStore.Models
{
    /// <summary>
    /// Describe a collection which can be queried after a statement and inserted after an item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class InOutDatabase<T> : IOutDatabase<T>, IEnumerable<T>, INotifyCollectionChanged
    {
        protected NotifyCollectionChangedEventHandler itemsProcessed;
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                itemsProcessed -= value;
                itemsProcessed += value;
            }

            remove
            {
                itemsProcessed -= value;
            }
        }

        public abstract IEnumerable<T> GetAll();
        public abstract IEnumerable<T> GetAllAfter(T afterWhich);
        public abstract IEnumerable<T> GetAllAfter(Predicate<T> prefix);
        public abstract void Add(T newItem);
        public abstract void InsertAfter(T afterWhich, T newItem);

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
