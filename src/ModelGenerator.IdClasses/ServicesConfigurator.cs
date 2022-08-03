using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelGenerator.Framework;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.CodeGeneration.FileTypes;
using ModelGenerator.IdClasses.CodeGeneration;
using ModelGenerator.IdClasses.Configuration;

namespace ModelGenerator.IdClasses
{
    public static class ServicesConfigurator
    {
        public static void Configure(IServiceCollection collection, IConfiguration configuration)
        {
            // Configuration
            collection.AddConfiguration<IdSettings>(configuration, "IdClasses");

            // Generators
            collection.AddSingleton<ITypeGenerator<DefaultFile>, FieldIdGenerator>()
                      .AddSingleton<ITypeGenerator<DefaultFile>, TemplateIdGenerator>()
                      .AddSingleton<IUsingGenerator<DefaultFile>, IdUsingGenerator>();
        }
    }
}