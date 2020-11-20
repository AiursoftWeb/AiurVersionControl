using Microsoft.EntityFrameworkCore;

namespace SampleWebApp.Data
{
    public class LogItem
    {
        public string Message { get; set; }

        public override string ToString()
        {
            return Message;
        }

        public override bool Equals(object obj)
        {
            return (obj as LogItem)?.Message == Message;
        }
    }

    public class InDbEntity
    {
        public int Id { get; set; }
        public string Content { get; set; }
    }

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<InDbEntity> InDbEntities { get; set; }
    }
}
