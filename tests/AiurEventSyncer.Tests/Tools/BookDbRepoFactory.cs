using AiurEventSyncer.Models;
using AiurEventSyncer.Tests.Models;
using AiurStore.Providers;
using Microsoft.EntityFrameworkCore;

namespace AiurEventSyncer.Tests.Tools
{
    public static class BookDbRepoFactory
    {
        private static readonly object _obj = new object();
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
            var store = new DbSetDb<Commit<Book>, Commit<Book>, SqlDbContext>(
                contextFactory: () => new SqlDbContext(),
                dbSetFactory: (context) => context.BookCommits,
                queryFactory: (dbSet) => dbSet.Include(t => t.Item),
                addAction: (newItem, dbSet) => dbSet.Add(newItem)
            );
            return new Repository<Book>(store);
        }

        public static Repository<int> BuildIntRepo()
        {
            var context = ResetDb();
            var store = new DbSetDb<Commit<int>, Commit<int>, SqlDbContext>(
                contextFactory: () => new SqlDbContext(),
                dbSetFactory: (context) => context.IntCommits,
                queryFactory: (dbSet) => dbSet,
                addAction: (newItem, dbSet) => dbSet.Add(newItem)
            );
            return new Repository<int>(store);
        }
    }
}
