using AiurEventSyncer.Models;
using SampleWebApp.Models;

namespace SampleWebApp.Services
{
    public class RepositoryFactory
    {
        public Repository<LogItem> BuildRepo()
        {
            return new Repository<LogItem>()
            {
                Name = "SERVER"
            };
        }
    }
}
