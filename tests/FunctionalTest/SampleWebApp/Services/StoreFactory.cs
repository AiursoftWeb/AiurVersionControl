using AiurEventSyncer.Models;
using AiurStore.Models;
using AiurStore.Providers.DbQueryProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SampleWebApp.Data;
using System.Linq;

namespace SampleWebApp.Services
{
    public class StoreFactory
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public StoreFactory(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public InOutDatabase<Commit<LogItem>> BuildStore()
        {
            return DbQueryProviderTools.BuildFromDbSet(
                contextFactory: () =>
                {
                    var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    return context;
                },
                queryFactory: (context) =>
                {
                    return context.InDbEntities.Include(t => t.Item);
                },
                addAction: (newItem, context) =>
                {
                    context.InDbEntities.Add(newItem);
                    context.SaveChanges();
                });
        }
    }
}
