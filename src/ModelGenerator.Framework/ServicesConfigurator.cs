using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelGenerator.Framework.Activities;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.Configuration;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Framework.FileScanning;
using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;
using TemplateFilter = ModelGenerator.Framework.FileScanning.TemplateFilter;

namespace ModelGenerator.Framework
{
    public static class ServicesConfigurator
    {
        public static void Configure(IServiceCollection collection, IConfiguration configuration)
        {
            // TODO: Move configuration into Common or activity-specific sections.
            collection
                .AddConfiguration<FieldIds>(configuration, nameof(FieldIds))
                .AddConfiguration<ItemParsingSettings>(configuration, "Common:ItemParsing")
                .AddConfiguration<PathFilterSettings>(configuration, "PathFilters")
                .AddConfiguration<TemplateIds>(configuration, nameof(TemplateIds))
                .AddConfiguration<Settings>(configuration, nameof(Settings))
                .AddConfiguration<XmlDocumentationSettings>(configuration, "XmlDocumentation");

            collection
                .AddSingleton<ICodeGenerator, CodeGenerator>()
                .AddSingleton<IDatabaseFactory, DatabaseFactory>()
                .AddSingleton<IFilePathFilter, TemplateFilter>()
                .AddSingleton<IItemFilter, PathFilter>()
                .AddSingleton<IProgressTracker, ProgressTracker>()
                .AddSingleton<ITemplateCollectionFactory, TemplateCollectionFactory>()
                .AddSingleton<ITypeFactory, TypeFactory>()
                .AddSingleton<IXmlDocumentationGenerator, XmlDocumentationGenerator>();

            collection
                .AddTransient<IRewriter, AccessorRewriter>()
                .AddTransient<IRewriter, BaseTypeListRewriter>()
                .AddTransient<IRewriter, SpacingRewriter>()
                .AddSingleton<Func<IEnumerable<IRewriter>>>(sp => sp.GetServices<IRewriter>);

            collection.AddSingleton(typeof(ProgressStep<>))
                      .AddSingleton<DatabaseActivity>()
                      .AddSingleton<FileParseActivity>()
                      .AddSingleton<FileScanActivity>()
                      .AddSingleton<GenerationActivity>()
                      .AddSingleton<TemplateActivity>()
                      .AddSingleton<TypeActivity>();
        }
    }
}