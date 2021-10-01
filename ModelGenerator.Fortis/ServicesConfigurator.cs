using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelGenerator.Fortis.CodeGeneration;
using ModelGenerator.Fortis.Configuration;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.TypeConstruction;

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
                      .AddSingleton<IGenerator<ModelClass, MemberDeclarationSyntax>, FortisClassGenerator>()
                      .AddSingleton<IGenerator<ModelFile>, FortisFileGenerator>()
                      .AddSingleton<IGenerator<ModelIdType, MemberDeclarationSyntax>, FortisIdGenerator>()
                      .AddSingleton<IGenerator<ModelInterface, MemberDeclarationSyntax>, FortisInterfaceGenerator>()
                      .AddSingleton<TypeNameResolver>()
                      .AddSingleton<XmlDocGenerator>();
        }
    }
}