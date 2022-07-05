﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace ModelGenerator
{
    public class ProviderSettings
    {
        public InputProviderNames Input { get; set; }
        public List<OutputProviderNames> Output { get; set; } = new();

        public ProviderSettings(IConfiguration configuration)
        {
            configuration.GetSection("Providers").Bind(this);

            // For backwards compatibility, the output provider may be a single value.
            if (Output.Count == 0)
            {
                var singleOutput = configuration.GetValue<OutputProviderNames>("Providers:Output");
                if (singleOutput != OutputProviderNames.Unspecified)
                {
                    Output.Add(singleOutput);
                }
            }
        }
    }
}