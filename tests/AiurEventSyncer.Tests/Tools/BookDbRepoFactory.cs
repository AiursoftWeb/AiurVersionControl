using AiurEventSyncer.Models;
using AiurEventSyncer.Tests.Models;
using AiurStore.Providers.DbQueryProvider;
using System.Linq;

namespace AiurEventSyncer.Tests.Tools
{
    public static class BookDbRepoFactory
    {

        public static SqlDbContext GetDbContext()
        {
            var _dbContext = new SqlDbContext();
            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();
            return _dbContext;
        }

        public static Repository<T> BuildRepo<T>()
        {
            var dbContext = GetDbContext();
            var store = DbQueryProviderTools.BuildFromDbSet<Commit<T>>(
                queryFactory: () => dbContext.Records.Where(t => t.Type == typeof(T).Name).Select(t => t.Content),
                addAction: (newItem) =>
                {
                    dbContext.Records.Add(new InDbEntity
                    {
                        Content = newItem,
                        Type = typeof(T).Name
                    });
                    dbContext.SaveChanges();
                });
            return new Repository<T>(store);
        }
    }
}
