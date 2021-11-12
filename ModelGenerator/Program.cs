using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Karambolo.Extensions.Logging.File;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                      .ConfigureLogging(ConfigureLogging)
                      .RunConsoleAsync();
        }

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder logBuilder)
        {
            const string logfileName = "modelGenerator.log";
            var logfile = Path.Combine(context.HostingEnvironment.ContentRootPath, logfileName);
            if (File.Exists(logfile))
            {
                File.WriteAllText(logfile, string.Empty);
            }

            logBuilder.ClearProviders();
            logBuilder.AddConfiguration(context.Configuration.GetSection("Logging"));
            logBuilder.AddFile(o =>
            {
                o.RootPath = context.HostingEnvironment.ContentRootPath;
                o.Files = new[]
                {
                    new LogFileOptions
                    {
                        DateFormat = "s",
                        Path = logfileName
                    }
                };
            });
        }

        private static void LoadConfiguration(HostBuilderContext context, IConfigurationBuilder configBuilder)
        {
            // Load main settings from file in application folder
            var assemblyLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            configBuilder.AddYamlFile(Path.Combine(assemblyLocation, "appSettings.yml"));

            // Load additional layer from working directory
            configBuilder.AddYamlFile(Path.Combine(context.HostingEnvironment.ContentRootPath, "modelGenerator.yml"), true);
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
        }
    }
}