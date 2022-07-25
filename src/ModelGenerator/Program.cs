using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Karambolo.Extensions.Logging.File;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelGenerator.Licensing;
using FortisServicesConfigurator = ModelGenerator.Fortis.ServicesConfigurator;
using FrameworkServicesConfigurator = ModelGenerator.Framework.ServicesConfigurator;
using IdClassesServicesConfigurator = ModelGenerator.IdClasses.ServicesConfigurator;
using ScsServicesConfigurator = ModelGenerator.Scs.ServicesConfigurator;
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

        /// <summary>
        /// Loads the configuration for the application from defined locations
        /// in a defined order (see method body).
        /// </summary>
        /// <param name="context">The builder context</param>
        /// <param name="configBuilder">The configuration builder</param>
        /// <exception cref="Exception">Throws an exception if the assembly location could not be resolved.</exception>
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

        /// <summary>
        /// Registers the services (using the appropriate services configurator) for
        /// the selected input provider
        /// </summary>
        /// <param name="providers">The provider settings from the configuration</param>
        /// <param name="hostBuilderContext">The host builder</param>
        /// <param name="collection">The current service collection</param>
        /// <exception cref="Exception">Thrown if the provider is not recognised</exception>
        private static void RegisterInputProvider(ProviderSettings providers, HostBuilderContext hostBuilderContext, IServiceCollection collection)
        {
            switch (providers.Input)
            {
                case InputProviderNames.Scs:
                    ScsServicesConfigurator.Configure(collection, hostBuilderContext.Configuration);
                    break;
                case InputProviderNames.Tds:
                    TdsServicesConfigurator.Configure(collection, hostBuilderContext.Configuration);
                    break;
                default:
                    throw new Exception($"Unsupported input provider: {providers.Input}");
            }
        }

        /// <summary>
        /// Registers the services (using the appropriate services configurator) for
        /// the selected output providers
        /// </summary>
        /// <param name="providers">The provider settings from the configuration</param>
        /// <param name="hostBuilderContext">The host builder</param>
        /// <param name="collection">The current service collection</param>
        /// <exception cref="Exception">Thrown if the provider is not recognised</exception>
        private static void RegisterOutputProviders(ProviderSettings providers, HostBuilderContext hostBuilderContext, IServiceCollection collection)
        {
            foreach (var providerName in providers.Output)
            {
                switch (providerName)
                {
                    case OutputProviderNames.Fortis:
                        FortisServicesConfigurator.Configure(collection, hostBuilderContext.Configuration);
                        break;
                    case OutputProviderNames.Ids:
                        IdClassesServicesConfigurator.Configure(collection, hostBuilderContext.Configuration);
                        break;
                    default:
                        throw new Exception($"Unsupported output provider: {providerName}");
                }
            }
        }

        /// <summary>
        /// Registers features and services.
        /// </summary>
        /// <param name="hostBuilderContext">Host builder</param>
        /// <param name="collection">Service collection</param>
        private static void RegisterServices(HostBuilderContext hostBuilderContext, IServiceCollection collection)
        {
            collection.AddOptions();

            FrameworkServicesConfigurator.Configure(collection, hostBuilderContext.Configuration);
            
            var providers = new ProviderSettings(hostBuilderContext.Configuration);
            RegisterInputProvider(providers, hostBuilderContext, collection);
            RegisterOutputProviders(providers, hostBuilderContext, collection);

            // These should be registered last just to ensure that they cannot be tampered with
            // by any of the provider configurators.
            collection
               .AddSingleton<LicenseManager>()
               .AddSingleton<Runner>()
               .AddHostedService<Worker>();
        }
    }
}