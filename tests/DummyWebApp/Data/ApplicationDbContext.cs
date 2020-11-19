using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DummyWebApp.Data
{
    public class LogItem
    {
        public string Message { get; set; }
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
