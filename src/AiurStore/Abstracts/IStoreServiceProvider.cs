using System.Collections.Generic;

namespace AiurStore.Abstracts
{
    public interface IStoreProvider
    {
        public IEnumerable<string> GetAll();
        public void Insert(string newItem);
        public void Drop();
    }
}
