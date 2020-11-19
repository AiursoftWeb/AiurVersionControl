using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurStore.Tests.TestDbs
{
    public class InDbEntity
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string Filter { get; set; }
    }
    public class SqliteDbContext : DbContext
    {
        public DbSet<InDbEntity> Records { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=test.db");
    }
}
