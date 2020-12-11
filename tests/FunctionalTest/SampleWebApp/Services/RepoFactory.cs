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
        private readonly IServiceScopeFactory _scopeFactory;

        public RepoFactory(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Repository<T> BuildRepo()
        {
            var store = DbQueryProviderTools.BuildFromDbSet<Commit<T>, ApplicationDbContext>(
                contextFactory: () =>
                {
                    var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    return context;
                },
                queryFactory: (context) =>
                {
                    return context.InDbEntities.Select(t => t.Content);
                },
                addAction: (newItem, context) =>
                {
                    context.InDbEntities.Add(new InDbEntity
                    {
                        Content = newItem
                    });
                    context.SaveChanges();
                });
#warning In Object and In DB>
            return new Repository<T>
            {
                Name = "Web Server"
            };
            //return new Repository<T>(store);
        }
    }
}
