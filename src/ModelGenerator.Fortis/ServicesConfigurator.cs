using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelGenerator.Fortis.CodeGeneration;
using ModelGenerator.Fortis.Configuration;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.CodeGeneration.FileTypes;

namespace ModelGenerator.Fortis
{
    public static class ServicesConfigurator
    {
        public static void Configure(IServiceCollection collection, IConfiguration configuration)
        {
            collection
                .Configure<FortisSettings>(opts => configuration.GetSection("Fortis").Bind(opts))
                .AddSingleton(sp => sp.GetRequiredService<IOptions<FortisSettings>>().Value);

            collection.AddSingleton<IFortisFieldNameResolver, FortisFieldNameResolver>()
                      .AddSingleton<FieldTypeResolver>()
                      .AddSingleton<TypeNameResolver>();

            collection.AddSingleton<IUsingGenerator<DefaultFile>, FortisUsingGenerator>()
                      .AddSingleton<ITypeGenerator<DefaultFile>, FortisClassGenerator>()
                      .AddSingleton<ITypeGenerator<DefaultFile>, FortisInterfaceGenerator>()
                      .AddSingleton<ITypeGenerator<DefaultFile>, FortisFieldIdGenerator>()
                      .AddSingleton<ITypeGenerator<DefaultFile>, FortisTemplateIdGenerator>();
        }
    }
}