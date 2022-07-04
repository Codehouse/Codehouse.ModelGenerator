using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Framework.FileScanning;
using ModelGenerator.Scs.FileParsing;
using ModelGenerator.Scs.FileScanning;

namespace ModelGenerator.Scs
{
    public static class ServicesConfigurator
    {
        public static void Configure(IServiceCollection collection, IConfiguration configuration)
        {
            collection
                .Configure<ScsSettings>(opts => configuration.GetSection("Scs").Bind(opts))
                .AddSingleton(sp => sp.GetRequiredService<IOptions<ScsSettings>>().Value);

            collection.AddSingleton<IFileParser, ScsFileParser>()
                      .AddSingleton<IFileScanner, ScsFileScanner>()
                      .AddSingleton<ISourceProvider, ScsSourceProvider>();
        }
    }
}