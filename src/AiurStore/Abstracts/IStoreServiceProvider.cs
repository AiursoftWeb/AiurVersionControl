using System.Collections.Generic;

namespace AiurStore.Abstracts
{
    public interface IStoreProvider
    {
        public IEnumerable<string> GetAll();
        public void Add(string newItem);
        public void Clear();
        public void Insert(int index, string newItem);
    }
}
