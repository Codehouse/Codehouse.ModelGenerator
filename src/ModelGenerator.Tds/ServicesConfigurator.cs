using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelGenerator.Framework;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Framework.FileScanning;
using ModelGenerator.Tds.Parsing;

namespace ModelGenerator.Tds
{
    public static class ServicesConfigurator
    {
        public static void Configure(IServiceCollection collection, IConfiguration configuration)
        {
            collection.AddConfiguration<TdsSettings>(configuration, "Tds");

            collection.AddSingleton<IFileParser, TdsFileParser>()
                      .AddSingleton<IFileScanner, TdsFileScanner>()
                      .AddSingleton<ISourceProvider, TdsSourceProvider>()
                      .AddSingleton<ITdsItemParser, TdsItemParser>()
                      .AddSingleton<ITdsTokenizer, TdsItemTokenizer>();
            // This class contains experimental namespace working - it isn't ready for use.
            //.AddSingleton<ITemplateCollectionFactory, TdsTemplateCollectionFactory>();
        }
    }
}