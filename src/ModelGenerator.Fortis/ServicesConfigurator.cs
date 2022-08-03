using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelGenerator.Fortis.CodeGeneration;
using ModelGenerator.Fortis.Configuration;
using ModelGenerator.Framework;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.CodeGeneration.FileTypes;

namespace ModelGenerator.Fortis
{
    public static class ServicesConfigurator
    {
        public static void Configure(IServiceCollection collection, IConfiguration configuration)
        {
            // Config
            collection.AddConfiguration<FortisSettings>(configuration, "Fortis");

            // Services & overrides
            collection.AddSingleton<IFortisFieldNameResolver, FortisFieldNameResolver>()
                      .AddSingleton<IFortisTypeNameResolver, FortisTypeNameResolver>()
                      .AddSingleton<FieldTypeResolver>();

            // Generators
            collection.AddSingleton<IUsingGenerator<DefaultFile>, FortisUsingGenerator>()
                      .AddSingleton<ITypeGenerator<DefaultFile>, FortisClassGenerator>()
                      .AddSingleton<ITypeGenerator<DefaultFile>, FortisInterfaceGenerator>();
        }
    }
}