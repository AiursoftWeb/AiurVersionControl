using System.Collections.Generic;

namespace AiurStore.Abstracts
{
    public interface IStoreProvider<T>
    {
        public IEnumerable<T> GetAll();
        public void Add(T newItem);
        public void Clear();
        public void Insert(int index, T newItem);
    }
}
