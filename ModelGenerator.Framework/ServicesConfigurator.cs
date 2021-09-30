﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Framework.FileScanning;
using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.TypeConstruction;
using TemplateFilter = ModelGenerator.Framework.FileParsing.TemplateFilter;

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
                .AddSingleton<ICodeGenerator, CodeGenerator>()
                .AddSingleton<IDatabaseFactory, DatabaseFactory>()
                .AddSingleton<IFilePathFilter, FileScanning.TemplateFilter>()
                .AddSingleton<IItemFilter, TemplateFilter>()
                .AddSingleton<ITemplateCollectionFactory, TemplateCollectionFactory>()
                .AddSingleton<ITypeFactory, TypeFactory>();
        }
    }
}