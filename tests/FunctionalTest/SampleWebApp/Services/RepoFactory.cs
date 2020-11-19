using AiurEventSyncer.Models;
using AiurStore.Providers.DbQueryProvider;
using SampleWebApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleWebApp.Services
{
    public class RepoFactory
    {
        private readonly ApplicationDbContext _dbContext;

        public RepoFactory(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Repository<T> BuildRepo<T>()
        {
            var store = DbQueryProviderTools.BuildFromDbSet<Commit<T>>(
                queryFactory: () => _dbContext.InDbEntities.Select(t => t.Content),
                addAction: (newItem) =>
                {
                    _dbContext.InDbEntities.Add(new InDbEntity
                    {
                        Content = newItem
                    });
                    _dbContext.SaveChanges();
                });
            return new Repository<T>(store);
        }
    }
}
