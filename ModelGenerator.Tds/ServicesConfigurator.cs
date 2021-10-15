using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Framework.FileScanning;
using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Tds.ItemModelling;
using ModelGenerator.Tds.Parsing;

namespace ModelGenerator.Tds
{
    public static class ServicesConfigurator
    {
        public static void Configure(IServiceCollection collection)
        {
            collection.AddSingleton<IFileParser, TdsFileParser>()
                      .AddSingleton<IFileScanner, TdsFileScanner>()
                      .AddSingleton<ITdsItemParser, TdsItemParser>()
                      .AddSingleton<ITdsTokenizer, TdsItemTokenizer>()
                      .AddSingleton<ITemplateCollectionFactory, TdsTemplateCollectionFactory>();
        }
    }
}