using Aiursoft.AiurEventSyncer.WebExtends;
using Aiursoft.AiurVersionControl.CRUD;
using Aiursoft.AiurVersionControl.SampleWPF.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows.Threading;

namespace Aiursoft.AiurVersionControl.SampleWPF.Services
{
    public class ServerProgram
    {
        public static IHost BuildHost(string[] args,
            CollectionRepository<Book> repo,
            Dispatcher dispatcher,
            int port = 15000)
        {
            return CreateHostBuilder(args, repo, dispatcher, port).Build();
        }

        public static IHostBuilder CreateHostBuilder(
            string[] args,
            CollectionRepository<Book> repo,
            Dispatcher dispatcher,
            int port)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls($"http://*:{port}");
                    webBuilder.UseStartup<ServerStartup>();
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddSingleton(dispatcher);
                        services.AddSingleton(repo);
                    });
                });
        }
    }

    public class ServerStartup
    {
        public void Configure(
            IApplicationBuilder app,
            CollectionRepository<Book> repo,
            Dispatcher dispatcher)
        {
            app.UseWebSockets();
            app.Use(async (HttpContext context, RequestDelegate _) =>
            {
                var start = context.Request.Query["start"];
                var result = dispatcher.Invoke(() => context.RepositoryAsync(repo, start));
                await result;
            });
        }
    }
}
