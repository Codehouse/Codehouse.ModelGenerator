using Microsoft.Extensions.DependencyInjection;
using ModelGenerator.Fortis.CodeGeneration;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Fortis
{
    public static class ServicesConfigurator
    {
        public static void Configure(IServiceCollection collection)
        {
            collection.AddSingleton<IGenerator<ModelClass>, FortisClassGenerator>()
                      .AddSingleton<IGenerator<ModelFile>, FortisFileGenerator>()
                      .AddSingleton<IGenerator<ModelIdType>, FortisIdGenerator>()
                      .AddSingleton<IGenerator<ModelInterface>, FortisInterfaceGenerator>();
        }
    }
}