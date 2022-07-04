using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.FileScanning;

namespace ModelGenerator.Scs
{
    public class ScsSourceProvider : SourceProviderBase
    {
        private readonly ScsSettings _settings;

        public ScsSourceProvider(ILogger<ScsSourceProvider> log, ScsSettings settings) : base(log)
        {
            _settings = settings;
        }

        public override IEnumerable<string> GetSources()
        {
            return GetSources(_settings.Sources, _settings.Root);
        }
    }
}