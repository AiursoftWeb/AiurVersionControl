using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SampleWebApp.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SampleWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildHost(args)
                .Run();
        }

        public static IHost BuildHost(string[] args, int port = 15000)
        {
            return CreateHostBuilder(args, port)
                .Build()
                .Reset<ApplicationDbContext>();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, int port)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(options =>
                    {
                        options.ListenAnyIP(port);
                    });
                    webBuilder.UseStartup<Startup>();
                });
        }
    }

    public static class Extends
    {
        public static IHost Reset<TContext>(this IHost host) where TContext : DbContext
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TContext>>();
            var context = services.GetService<TContext>();
            try
            {
                logger.LogInformation($"Migrating database associated with context {typeof(TContext).Name}");
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred while migrating the database used on context {typeof(TContext).Name}");
            }
            return host;
        }
    }
}
