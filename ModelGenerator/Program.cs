using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelGenerator.Framework;

namespace ModelGenerator
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                      .ConfigureServices(RegisterServices)
                      .RunConsoleAsync();
        }

        private static void RegisterServices(HostBuilderContext hostBuilderContext, IServiceCollection collection)
        {
            ServicesConfigurator.Configure(collection, hostBuilderContext.Configuration);
            Fortis.ServicesConfigurator.Configure(collection);
            Tds.ServicesConfigurator.Configure(collection);

            collection.AddOptions()
                      .Configure<Settings>(opts => hostBuilderContext.Configuration.GetSection(nameof(Settings)).Bind(opts))
                      .AddSingleton<Runner>()
                      .AddHostedService<Worker>();
            //collection.AddHostedService<TestWorker>();
        }
    }
}