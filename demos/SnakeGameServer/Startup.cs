using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SnakeGameServer.Controllers;

namespace SnakeGameServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSingleton<RepositoryContainer>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseWebSockets();
            app.UseRouting();
            app.UseEndpoints(endpoint => endpoint.MapDefaultControllerRoute());
        }
    }
}
