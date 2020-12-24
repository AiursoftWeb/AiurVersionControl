using AiurEventSyncer.Models;
using Microsoft.EntityFrameworkCore;

namespace AiurEventSyncer.Tests.Models
{
    public class SqlDbContext : DbContext
    {
        public DbSet<Commit<Book>> BookCommits { get; set; }
        public DbSet<Commit<int>> IntCommits { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=test.db");
    }
}
