using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlobExpressions;
using Microsoft.Extensions.Logging;

namespace ModelGenerator.Framework.FileScanning
{
    public abstract class SourceProviderBase : ISourceProvider
    {
        private readonly ILogger<SourceProviderBase> _log;

        protected SourceProviderBase(ILogger<SourceProviderBase> log)
        {
            _log = log;
        }

        public abstract IEnumerable<string> GetSources();

        protected IEnumerable<string> GetSources(IEnumerable<string> sourcePatterns, string rootPath)
        {
            return sourcePatterns
                  .SelectMany(p => EvaluatePattern(p, rootPath))
                  .ToArray();
        }

        private IEnumerable<string> EvaluatePattern(string pattern, string rootPath)
        {
            var matchedFiles = Glob.Files(rootPath, pattern)
                                   .Select(path => Path.Combine(rootPath, path))
                                   .ToList();
            _log.LogInformation("Pattern {pattern} located {matches} files.", pattern, matchedFiles.Count);
            return matchedFiles;
        }
    }
}