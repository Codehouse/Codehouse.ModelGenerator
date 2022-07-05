using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelGenerator.Framework.Activities;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.CodeGeneration.FileTypes;
using ModelGenerator.Framework.Configuration;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Framework.FileParsing.ItemFilters;
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
            // Config registrations
            collection
               .AddConfiguration<FieldIds>(configuration, nameof(FieldIds))
               .AddConfiguration<CodeGenerationSettings>(configuration, "Common:CodeGeneration")
               .AddConfiguration<ItemParsingSettings>(configuration, "Common:ItemParsing")
               .AddConfiguration<PathFilterSettings>(configuration, "PathFilters")
               .AddConfiguration<TemplateIds>(configuration, nameof(TemplateIds))
               .AddConfiguration<Settings>(configuration, nameof(Settings))
               .AddConfiguration<XmlDocumentationSettings>(configuration, "XmlDocumentation");

            // Service registrations
            collection
               .AddSingleton<IDatabaseFactory, DatabaseFactory>()
               .AddSingleton<IFilePathFilter, TemplateFilter>()
               .AddSingleton<IFieldNameResolver, FieldNameResolver>()
               .AddSingleton<IItemFilter, PathFilter>()
               .AddSingleton<IItemFilter, FieldItemHaseTypeFilter>()
               .AddSingleton<ITemplateCollectionFactory, TemplateCollectionFactory>()
               .AddSingleton<ITypeFactory, TypeFactory>()
               .AddSingleton<ITypeNameResolver, TypeNameResolver>()
               .AddSingleton<IXmlDocumentationGenerator, XmlDocumentationGenerator>();

            // Default file type infrastructure
            collection
               .AddSingleton<IFileFactory, DefaultFileFactory>()
               .AddSingleton<IFileGenerator, DefaultFileGenerator<DefaultFile>>();

            // Progress tracker specific registrations (note lifetimes)
            collection
               .AddTransient<IProgressTracker, ProgressTracker>()
               .AddSingleton(sp => new Lazy<IProgressTracker>(() => sp.GetRequiredService<IProgressTracker>()));

            // Rewriter registrations
            collection
               .AddTransient<IRewriter, AccessorRewriter>()
               .AddTransient<IRewriter, BaseTypeListRewriter>()
               .AddTransient<IRewriter, SpacingRewriter>()
               .AddSingleton<Func<IEnumerable<IRewriter>>>(sp => sp.GetServices<IRewriter>);

            // Activities
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