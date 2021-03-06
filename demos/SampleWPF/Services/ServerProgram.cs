using AiurEventSyncer.Models;
using AiurEventSyncer.WebExtends;
using AiurVersionControl.CRUD;
using AiurVersionControl.SampleWPF.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace AiurVersionControl.SampleWPF.Services
{
    public class ServerProgram
    {
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
                    webBuilder.UseStartup<ServerStartup>();
                });
        }
    }

    public class ServerStartup
    {
        public IConfiguration Configuration { get; }
        public ServerStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseWebSockets();
            app.UseRouting();
            app.UseEndpoints(endpoint => endpoint.MapDefaultControllerRoute());
            app.UseWelcomePage();
        }
    }

    public class AppController : Controller
    {
        public static CollectionRepository<Book> Repo { get; set; }

        [Route("repo.ares")]
        public Task<IActionResult> Index(string start)
        {
            return this.RepositoryAsync(Repo, start);
        }
    }
}
