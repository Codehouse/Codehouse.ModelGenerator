using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlobExpressions;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.FileScanning;

namespace ModelGenerator.Tds
{
    public class TdsSourceProvider : ISourceProvider
    {
        private readonly ILogger<TdsSourceProvider> _log;
        private readonly TdsSettings _settings;

        public TdsSourceProvider(ILogger<TdsSourceProvider> log, TdsSettings settings)
        {
            _log = log;
            _settings = settings;
        }

        public IEnumerable<string> GetSources()
        {
            return _settings.Sources
                            .SelectMany(EvaluatePattern)
                            .ToArray();
        }

        private IEnumerable<string> EvaluatePattern(string pattern)
        {
            var matchedFiles = Glob.Files(_settings.Root, pattern)
                                   .Select(path => Path.Combine(_settings.Root, path))
                                   .ToList();
            _log.LogInformation($"Pattern {pattern} located {matchedFiles.Count} files.");
            return matchedFiles;
        }
    }
}