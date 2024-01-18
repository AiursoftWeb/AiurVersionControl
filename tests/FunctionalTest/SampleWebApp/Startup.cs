using Aiursoft.WebTools.Abstractions.Models;
using SampleWebApp.Services;

namespace SampleWebApp
{
    public class Startup : IWebStartup
    {
        public void ConfigureServices(IConfiguration configuration, IWebHostEnvironment environment, IServiceCollection services)
        {
            services.AddControllers().AddApplicationPart(typeof(Startup).Assembly);
            services.AddSingleton<RepositoryContainer>();
            services.AddSingleton<RepositoryFactory>();
        }

        public void Configure(WebApplication app)
        {
            app.UseDeveloperExceptionPage();
            app.UseWebSockets();
            app.UseRouting();
            app.MapDefaultControllerRoute();
        }
    }
}
