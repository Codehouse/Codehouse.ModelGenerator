using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelGenerator.Framework.FileScanning;

namespace ModelGenerator
{
    public class Runner
    {
        private readonly IOptions<Settings> _settings;
        private readonly IFileScanner _fileScanner;
        private readonly ILogger<Runner> _logger;

        public Runner(IOptions<Settings> settings, IFileScanner fileScanner, ILogger<Runner> logger)
        {
            _settings = settings;
            _fileScanner = fileScanner;
            _logger = logger;
        }
        
        public async Task RunAsync(CancellationToken stoppingToken)
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
            
            var fileSets = patterns.SelectMany(path => _fileScanner.FindFilesInPath(root, path));
            foreach (var fileSet in fileSets)
            {
                stoppingToken.ThrowIfCancellationRequested();
                _logger.LogInformation($"Found {fileSet.Files.Count} files in {fileSet.Name}");
            }
        }
    }
}