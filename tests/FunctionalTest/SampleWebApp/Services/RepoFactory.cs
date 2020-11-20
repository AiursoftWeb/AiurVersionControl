using AiurEventSyncer.Models;
using AiurStore.Providers.DbQueryProvider;
using Microsoft.Extensions.DependencyInjection;
using SampleWebApp.Data;
using System;
using System.Linq;

namespace SampleWebApp.Services
{
    public class RepoFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public RepoFactory(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Repository<T> BuildRepo<T>()
        {
            var store = DbQueryProviderTools.BuildFromDbSet<Commit<T>>(
                queryFactory: () => _serviceProvider.GetRequiredService<ApplicationDbContext>().InDbEntities.Select(t => t.Content),
                addAction: (newItem) =>
                {
                    var db = _serviceProvider.GetRequiredService<ApplicationDbContext>();
                    db.InDbEntities.Add(new InDbEntity
                    {
                        Content = newItem
                    });
                    db.SaveChanges();
                });
            return new Repository<T>(store);
        }
    }
}
