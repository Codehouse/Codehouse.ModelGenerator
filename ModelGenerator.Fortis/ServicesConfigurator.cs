using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelGenerator.Fortis.CodeGeneration;
using ModelGenerator.Fortis.Configuration;
using ModelGenerator.Framework.CodeGeneration;

namespace ModelGenerator.Fortis
{
    public static class ServicesConfigurator
    {
        public static void Configure(IServiceCollection collection, IConfiguration configuration)
        {
            collection
                .Configure<FortisSettings>(opts => configuration.GetSection("Fortis").Bind(opts))
                .AddSingleton(sp => sp.GetRequiredService<IOptions<FortisSettings>>().Value);

            collection.AddSingleton<FieldNameResolver>()
                      .AddSingleton<FieldTypeResolver>()
                      .AddSingleton<FortisClassGenerator>()
                      .AddSingleton<IFileGenerator, FortisFileGenerator>()
                      .AddSingleton<FortisIdGenerator>()
                      .AddSingleton<FortisInterfaceGenerator>()
                      .AddSingleton<TypeNameResolver>();
        }
    }
}