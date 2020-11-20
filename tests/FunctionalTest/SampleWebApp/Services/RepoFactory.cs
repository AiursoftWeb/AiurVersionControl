using AiurEventSyncer.Models;
using AiurStore.Providers.DbQueryProvider;
using Microsoft.Extensions.DependencyInjection;
using SampleWebApp.Data;
using System;
using System.Linq;

namespace SampleWebApp.Services
{
    public class RepoFactory<T>
    {
        private readonly ApplicationDbContext _dbContext;

        public RepoFactory(
            ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Repository<T> BuildRepo()
        {
            var store = DbQueryProviderTools.BuildFromDbSet<Commit<T>>(
                queryFactory: () =>
                {
                    return _dbContext.InDbEntities.Select(t => t.Content);
                },
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
