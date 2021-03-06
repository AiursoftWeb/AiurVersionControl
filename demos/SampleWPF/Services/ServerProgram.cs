using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            app.UseWelcomePage();
            app.UseEndpoints(endpoint => endpoint.MapDefaultControllerRoute());
        }
    }

    public class HomeController : Controller
    {
        [Route("repo.ares")]
        public IActionResult Index()
        {
            return Json(new { });
        }
    }
}
