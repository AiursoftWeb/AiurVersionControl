using AiurEventSyncer.Models;
using AiurStore.Models;
using AiurStore.Providers.DbQueryProvider;
using Microsoft.Extensions.DependencyInjection;
using SampleWebApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleWebApp.Services
{
    public class StoreFactory
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public StoreFactory(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public InOutDatabase<Commit<T>> BuildStore<T>()
        {
            return DbQueryProviderTools.BuildFromDbSet<Commit<T>, ApplicationDbContext>(
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
        }
    }
}
