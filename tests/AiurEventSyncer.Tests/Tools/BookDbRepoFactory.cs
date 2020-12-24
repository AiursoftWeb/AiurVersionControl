using AiurEventSyncer.Models;
using AiurEventSyncer.Tests.Models;
using AiurStore.Providers.DbQueryProvider;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AiurEventSyncer.Tests.Tools
{
    public static class BookDbRepoFactory
    {
        private static object _obj = new object();
        public static SqlDbContext ResetDb()
        {
            lock (_obj)
            {
                var _dbContext = new SqlDbContext();
                _dbContext.Database.EnsureDeleted();
                _dbContext.Database.EnsureCreated();
                return _dbContext;
            }
        }

        public static Repository<Book> BuildBookRepo()
        {
            var context = ResetDb();
            var store = DbQueryProviderTools.BuildFromDbSet(
                contextFactory: () => new SqlDbContext(),
                queryFactory: (context) => context.BookCommits.Include(t => t.Item),
                addAction: (newItem, context) =>
                {
                    context.BookCommits.Add(newItem);
                    context.SaveChanges();
                });
            return new Repository<Book>(store)
            {
                Name = "Server"
            };
        }

        public static Repository<int> BuildIntRepo()
        {
            var context = ResetDb();
            var store = DbQueryProviderTools.BuildFromDbSet(
                contextFactory: () => new SqlDbContext(),
                queryFactory: (context) => context.IntCommits,
                addAction: (newItem, context) =>
                {
                    context.IntCommits.Add(newItem);
                    context.SaveChanges();
                });
            return new Repository<int>(store)
            {
                Name = "Server"
            };
        }
    }
}
