using AiurEventSyncer.Models;

namespace SampleWebApp.Services
{
    public class RepositoryFactory<T>
    {
        private readonly StoreFactory _storeFactory;

        public RepositoryFactory(StoreFactory storeFactory)
        {
            _storeFactory = storeFactory;
        }

        public Repository<T> BuildRepo()
        {
            var store = _storeFactory.BuildStore<T>();
            return new Repository<T>(store)
            {
                Name = "SERVER"
            };
        }
    }
}
