using Microsoft.Extensions.DependencyInjection;
using ModelGenerator.Framework.FileScanning;

namespace ModelGenerator.Framework
{
    public static class ServicesConfigurator
    {
        public static void Configure(IServiceCollection collection)
        {
            collection.AddSingleton<IFilePathFilter, TemplateFilter>();
        }
    }
}