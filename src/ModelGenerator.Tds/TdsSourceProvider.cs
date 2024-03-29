﻿using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.FileScanning;

namespace ModelGenerator.Tds
{
    public class TdsSourceProvider : SourceProviderBase
    {
        private readonly TdsSettings _settings;

        public TdsSourceProvider(ILogger<TdsSourceProvider> log, TdsSettings settings) : base(log)
        {
            _settings = settings;
        }

        public override IEnumerable<string> GetSources()
        {
            return GetSources(_settings.Sources, _settings.Root);
        }
    }
}