using System;
using System.IO;
using System.Reflection;
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
                      .ConfigureAppConfiguration(LoadConfiguration)
                      .ConfigureServices(RegisterServices)
                      .RunConsoleAsync();
        }

        private static void LoadConfiguration(HostBuilderContext context, IConfigurationBuilder configBuilder)
        {
            // Load main settings from file in application folder
            var assemblyLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            configBuilder.AddYamlFile(Path.Combine(assemblyLocation, "appSettings.yml"));
            
            // Load additional layer from working directory
            configBuilder.AddYamlFile(Path.Combine(context.HostingEnvironment.ContentRootPath, "modelGenerator.yml"), optional: true);
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