using AiurEventSyncer.Models;
using Microsoft.EntityFrameworkCore;

namespace SampleWebApp.Data
{
    public class LogItem
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return Message;
        }

        public override bool Equals(object obj)
        {
            return (obj as LogItem)?.Message == Message;
        }

        public override int GetHashCode()
        {
            return Message.GetHashCode();
        }
    }

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Commit<LogItem>> InDbEntities { get; set; }
    }
}
