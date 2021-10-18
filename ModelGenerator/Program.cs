using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelGenerator.Framework;

namespace ModelGenerator
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            // TODO: Convert config to YML
            // TODO: Add config layer for working directory
            await Host.CreateDefaultBuilder(args)
                      .ConfigureServices(RegisterServices)
                      .RunConsoleAsync();
        }

        private static void RegisterServices(HostBuilderContext hostBuilderContext, IServiceCollection collection)
        {
            collection.AddOptions();
            
            ServicesConfigurator.Configure(collection, hostBuilderContext.Configuration);
            Fortis.ServicesConfigurator.Configure(collection, hostBuilderContext.Configuration);
            Tds.ServicesConfigurator.Configure(collection);

            collection
                .AddSingleton<Runner>()
                .AddHostedService<Worker>();
            //collection.AddHostedService<TestWorker>();
        }
    }
}