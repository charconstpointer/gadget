using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Gadget.Notifications
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://+:5000");
                    webBuilder.UseStartup<Startup>();
                });
    }
}