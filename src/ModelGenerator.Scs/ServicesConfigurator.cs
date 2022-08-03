using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelGenerator.Framework;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Framework.FileScanning;
using ModelGenerator.Scs.FileParsing;
using ModelGenerator.Scs.FileScanning;

namespace ModelGenerator.Scs
{
    public static class ServicesConfigurator
    {
        public static void Configure(IServiceCollection collection, IConfiguration configuration)
        {
            // Configuration
            collection.AddConfiguration<ScsSettings>(configuration, "Scs");

            // Services & overrides
            collection.AddSingleton<IFileParser, ScsFileParser>()
                      .AddSingleton<IFileScanner, ScsFileScanner>()
                      .AddSingleton<ISourceProvider, ScsSourceProvider>();
        }
    }
}