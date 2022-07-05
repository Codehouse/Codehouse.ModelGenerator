using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            collection
               .Configure<IdSettings>(opts => configuration.GetSection("IdClasses").Bind(opts))
               .AddSingleton(sp => sp.GetRequiredService<IOptions<IdSettings>>().Value);

            collection.AddSingleton<ITypeGenerator<DefaultFile>, FieldIdGenerator>()
                      .AddSingleton<ITypeGenerator<DefaultFile>, TemplateIdGenerator>();
        }
    }
}