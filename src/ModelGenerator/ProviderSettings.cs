using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace ModelGenerator
{
    /// <summary>
    /// This class reads and interprets the providers configuration
    /// section.
    /// </summary>
    /// <remarks>It is not registered with the DI container</remarks>
    public class ProviderSettings
    {
        public string? Input { get; set; }
        public List<string?> Output { get; set; } = new();

        public ProviderSettings(IConfiguration configuration)
        {
            configuration.GetSection("Providers").Bind(this);

            // For backwards compatibility, the output provider may be a single value.
            if (Output.Count == 0)
            {
                var singleOutput = configuration.GetValue<string>("Providers:Output");
                if (!string.IsNullOrWhiteSpace(singleOutput))
                {
                    Output.Add(singleOutput);
                }
            }
        }
    }
}