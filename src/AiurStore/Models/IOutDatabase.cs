using System;
using System.Collections.Generic;

namespace AiurStore.Models
{
    /// <summary>
    /// Describe a collection which can be queried after a statement.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOutDatabase<T> : IEnumerable<T>
    {
        public IEnumerable<T> GetAll();
        public IEnumerable<T> GetAllAfter(T afterWhich);
        public IEnumerable<T> GetAllAfter(Predicate<T> prefix);
    }
}
