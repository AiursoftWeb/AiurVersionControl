using System;
using System.Collections;
using System.Collections.Generic;

namespace AiurStore.Models
{
    public abstract class InOutDatabase<T> : IEnumerable<T>
    {
        public abstract IEnumerable<T> GetAll();
        public abstract IEnumerable<T> GetAllAfter(T afterWhich);
        public abstract IEnumerable<T> GetAllAfter(Func<T, bool> prefix);
        public abstract void Add(T newItem);
        public abstract void InsertAfter(T afterWhich, T newItem);
        public abstract void Clear();

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
