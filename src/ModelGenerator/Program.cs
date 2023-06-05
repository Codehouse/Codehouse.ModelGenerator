using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Karambolo.Extensions.Logging.File;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FortisServicesConfigurator = ModelGenerator.Fortis.ServicesConfigurator;
using FrameworkServicesConfigurator = ModelGenerator.Framework.ServicesConfigurator;
using TdsServicesConfigurator = ModelGenerator.Tds.ServicesConfigurator;

namespace ModelGenerator
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                await Host.CreateDefaultBuilder(args)
                          .ConfigureAppConfiguration(LoadConfiguration)
                          .ConfigureServices(RegisterServices)
                          .ConfigureLogging(ConfigureLogging)
                          .RunConsoleAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled exception:");
                Console.WriteLine(ex);
            }
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
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
            {
                throw new Exception("Entry assembly was null.");
            }

            // Load main settings from file in application folder
            var assemblyLocation = Path.GetDirectoryName(entryAssembly.Location);
            if (assemblyLocation == null)
            {
                throw new Exception("Entry assembly location could not be determined.");
            }

            // Load configuration files from:
            // 1. Tool directory
            // 2. User directory
            // 3. Working directory
            // 4. Working directory user overrides
            configBuilder.AddYamlFile(Path.Combine(assemblyLocation, "appSettings.yml"))
                .AddYamlFile(Environment.ExpandEnvironmentVariables("%USERPROFILE%/modelGenerator.yml"), true)
                .AddYamlFile(Path.Combine(context.HostingEnvironment.ContentRootPath, "modelGenerator.yml"), true)
                .AddYamlFile(Path.Combine(context.HostingEnvironment.ContentRootPath, "modelGenerator.user.yml"), true);
        }

        private static void RegisterInputProvider(HostBuilderContext hostBuilderContext, IServiceCollection collection)
        {
            var providerName = hostBuilderContext.Configuration.GetValue<ProviderNames>("Providers:Input");
            switch (providerName)
            {
                case ProviderNames.Tds:
                    TdsServicesConfigurator.Configure(collection, hostBuilderContext.Configuration);
                    break;
                default:
                    throw new Exception($"Unsupported input provider: {providerName}");
            }
        }

        private static void RegisterOutputProvider(HostBuilderContext hostBuilderContext, IServiceCollection collection)
        {
            var providerName = hostBuilderContext.Configuration.GetValue<ProviderNames>("Providers:Output");
            switch (providerName)
            {
                case ProviderNames.Fortis:
                    FortisServicesConfigurator.Configure(collection, hostBuilderContext.Configuration);
                    break;
                default:
                    throw new Exception($"Unsupported output provider: {providerName}");
            }
        }

        private static void RegisterServices(HostBuilderContext hostBuilderContext, IServiceCollection collection)
        {
            collection.AddOptions();

            FrameworkServicesConfigurator.Configure(collection, hostBuilderContext.Configuration);
            RegisterInputProvider(hostBuilderContext, collection);
            RegisterOutputProvider(hostBuilderContext, collection);

            collection
                .AddSingleton<Runner>()
                .AddHostedService<Worker>();
        }
    }
}