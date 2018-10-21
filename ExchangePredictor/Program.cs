using ExchangePredictor.Services;
using ExchangePredictor.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ExchangePredictor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // create service collection
            var services = new ServiceCollection();
            ConfigureServices(services);

            // create service provider
            var serviceProvider = services.BuildServiceProvider();

            // entry to run app
            await serviceProvider.GetService<App>().RunAsync();

            Console.ReadLine();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new LoggerFactory()
                .AddConsole()
                .AddDebug());

            // add logging
            services.AddLogging();

            // build configuration
            IConfiguration configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", false)
              .Build();

            services.AddOptions();
            services.Configure<OpenExchangeSettings>(configuration.GetSection("OpenExchangeSettings"));

            // add services
            services.AddTransient<IPredictor, Predictor>();

            // add app
            services.AddTransient<App>();
        }
    }
}
