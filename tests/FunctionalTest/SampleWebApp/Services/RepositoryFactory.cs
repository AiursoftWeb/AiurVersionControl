using AiurEventSyncer.Models;
using SampleWebApp.Data;

namespace SampleWebApp.Services
{
    public class RepositoryFactory
    {
        private readonly StoreFactory _storeFactory;

        public RepositoryFactory(StoreFactory storeFactory)
        {
            _storeFactory = storeFactory;
        }

        public Repository<LogItem> BuildRepo()
        {
            var store = _storeFactory.BuildStore();
            return new Repository<LogItem>(store)
            {
                Name = "SERVER"
            };
        }
    }
}
