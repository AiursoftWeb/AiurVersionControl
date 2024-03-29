﻿using Aiursoft.AiurEventSyncer.Models;
using SampleWebApp.Models;

namespace SampleWebApp.Services
{
    public class RepositoryContainer 
    {
        private readonly RepositoryFactory _repositoryFactory;
        private static IServiceScopeFactory _serviceScopeFactory;
        private readonly object _obj = new();
        private Repository<LogItem> _logItemRepository;

        public RepositoryContainer(
            RepositoryFactory repositoryFactory,
            IServiceScopeFactory serviceScopeFactory)
        {
            _repositoryFactory = repositoryFactory;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Repository<LogItem> GetLogItemRepository()
        {
            lock (_obj)
            {
                if (_logItemRepository == null)
                {
                    _logItemRepository = _repositoryFactory.BuildRepo();
                }
            }
            return _logItemRepository;
        }

        public static Repository<LogItem> GetRepositoryForTest()
        {
            var scope = _serviceScopeFactory.CreateScope();
            var self = scope.ServiceProvider.GetRequiredService<RepositoryContainer>();
            return self.GetLogItemRepository();
        }

        public static void ResetRepositoryForTest()
        {
            var scope = _serviceScopeFactory.CreateScope();
            var self = scope.ServiceProvider.GetRequiredService<RepositoryContainer>();
            self._logItemRepository = null;
        }
    }
}
