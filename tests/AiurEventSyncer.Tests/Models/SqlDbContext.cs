using Microsoft.EntityFrameworkCore;

namespace AiurEventSyncer.Tests.Models
{
    public class InDbEntity
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
    }

    public class SqlDbContext : DbContext
    {
        public DbSet<InDbEntity> Records { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=test.db");
    }
}
