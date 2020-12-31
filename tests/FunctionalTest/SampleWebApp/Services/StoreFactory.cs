using AiurEventSyncer.Models;
using AiurStore.Models;
using AiurStore.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SampleWebApp.Data;

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
            return new DbSetDb<Commit<LogItem>, Commit<LogItem>, ApplicationDbContext>(
                contextFactory: () => 
                {
                    var scope = _scopeFactory.CreateScope();
                    return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                },
                dbSetFactory: (context) =>
                {
                    return context.InDbEntities;
                },
                queryFactory: (context) =>
                {
                    return context.Include(t => t.Item);
                },
                addAction: (newItem, context) =>
                {
                    context.Add(newItem);
                });
        }
    }
}
