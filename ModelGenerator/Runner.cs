using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Framework.FileScanning;

namespace ModelGenerator
{
    public class Runner
    {
        private readonly IOptions<Settings> _settings;
        private readonly IFileScanner _fileScanner;
        private readonly IFileParser _fileParser;
        private readonly ILogger<Runner> _logger;

        public Runner(IOptions<Settings> settings,
            IFileScanner fileScanner,
            IFileParser fileParser,
            ILogger<Runner> logger)
        {
            _settings = settings;
            _fileScanner = fileScanner;
            _fileParser = fileParser;
            _logger = logger;
        }
        
        public async Task RunAsync(CancellationToken stoppingToken)
        {
            var fileSets = await GetRawItems(stoppingToken);
            
            foreach (var fileSet in fileSets)
            {
                stoppingToken.ThrowIfCancellationRequested();
                _logger.LogInformation($"Found {fileSet.Files.Count} files in {fileSet.Name}");
                foreach (var file in fileSet.Files)
                {
                    await _fileParser.ParseFile(file);
                }
            }
        }

        private async Task<IEnumerable<FileSet>> GetRawItems(CancellationToken stoppingToken)
        {
            var patterns = _settings.Value.Patterns ?? Enumerable.Empty<string>();
            if (!_settings.Value.Patterns.Any())
            {
                throw new InvalidOperationException("There are no patterns configured.");
            }

            var root = _settings.Value.Root;
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                throw new InvalidOperationException("The root folder does not exist.");
            }

            return patterns.SelectMany(path => _fileScanner.FindFilesInPath(root, path));
        }

        private IEnumerable<ItemSet> GetParsedItems(IEnumerable<FileSet> fileSets)
        {
            throw new NotImplementedException();
        }
    }
}