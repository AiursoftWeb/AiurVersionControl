using Microsoft.EntityFrameworkCore;

namespace AiurStore.Tests.TestDbs
{
    public class InDbEntity
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string Filter { get; set; }
    }
    public class SqlDbContext : DbContext
    {
        public DbSet<InDbEntity> Records { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=test.db");
    }
}
