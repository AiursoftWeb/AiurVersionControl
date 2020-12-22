using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SnakeGameServer
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
                .Build();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, int port)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls($"http://localhost:{port}");
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
