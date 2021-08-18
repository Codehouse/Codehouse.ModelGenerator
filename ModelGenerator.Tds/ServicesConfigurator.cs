using Microsoft.Extensions.DependencyInjection;
using ModelGenerator.Framework.FileScanning;

namespace ModelGenerator.Tds
{
    public static class ServicesConfigurator
    {
        public static void Configure(IServiceCollection collection)
        {
            collection.AddSingleton<IFileScanner, TdsFileScanner>();
        }
    }
}