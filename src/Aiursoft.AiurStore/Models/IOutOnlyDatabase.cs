using System;
using System.Collections.Generic;

namespace Aiursoft.AiurStore.Models
{
    /// <summary>
    /// Describe a collection which can be queried after a statement.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOutOnlyDatabase<T> : IReadOnlyCollection<T>
    {
        public IEnumerable<T> GetAll();
        public IEnumerable<T> GetAllAfter(T afterWhich);
        public IEnumerable<T> GetAllAfter(Predicate<T> prefix);
    }
}
