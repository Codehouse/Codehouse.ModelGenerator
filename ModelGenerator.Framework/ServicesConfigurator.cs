using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Framework.FileScanning;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Framework
{
    public static class ServicesConfigurator
    {
        public static void Configure(IServiceCollection collection, IConfiguration configuration)
        {
            collection
                .Configure<FieldIds>(opts => configuration.GetSection(nameof(FieldIds)).Bind(opts))
                .AddSingleton<IFieldIds, FieldIds>(sp => sp.GetRequiredService<IOptions<FieldIds>>().Value)
                .Configure<TemplateIds>(opts => configuration.GetSection(nameof(TemplateIds)).Bind(opts))
                .AddSingleton<ITemplateIds, TemplateIds>(sp => sp.GetRequiredService<IOptions<TemplateIds>>().Value);

            collection
                .AddSingleton<IDatabaseFactory, DatabaseFactory>()
                .AddSingleton<IFilePathFilter, FileScanning.TemplateFilter>()
                .AddSingleton<IItemFilter, FileParsing.TemplateFilter>()
                .AddSingleton<ITemplateCollectionFactory, TemplateCollectionFactory>();
        }
    }
}