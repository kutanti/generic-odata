using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace EntitySchemaGenerator
{
    public class Program
    {
        public static IConfigurationRoot? Configuration { get; private set; }

        static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>

        Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, configuration) =>
        {
            configuration.Sources.Clear();
            configuration
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configurationRoot = configuration.Build();
            Configuration = configurationRoot;
        })
        .ConfigureServices((services) => Startup.ConfigureServices(services, Configuration 
            ?? throw new ArgumentException(nameof(Configuration))));

    }
}
