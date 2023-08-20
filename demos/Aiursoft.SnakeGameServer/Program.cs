namespace Aiursoft.SnakeGameServer
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
