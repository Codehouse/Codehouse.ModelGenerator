using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GlobExpressions;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.FileScanning;
using ModelGenerator.Framework.Progress;

namespace ModelGenerator.Framework.Activities
{
    public class FileScanActivity : CollectionActivityBase<string, FileSet>
    {
        public override string Description => "Scanning files";

        private readonly IFileScanner _fileScanner;
        private readonly ILogger<FileScanActivity> _logger;
        private string _root;

        public FileScanActivity(
            ILogger<FileScanActivity> logger,
            IFileScanner fileScanner)
        {
            _logger = logger;
            _fileScanner = fileScanner;
        }

        public override void SetInput(IEnumerable<string> input)
        {
            if (!input.Any())
            {
                throw new InvalidOperationException("There are no patterns configured.");
            }

            if (string.IsNullOrEmpty(_root))
            {
                throw new InvalidOperationException("Root path has not been configured.");
            }

            base.SetInput(input.SelectMany(EvaluatePattern));
        }

        public void SetRoot(string rootPath)
        {
            if (string.IsNullOrEmpty(rootPath))
            {
                SetRoot(Directory.GetCurrentDirectory());
                return;
            }

            if (!Directory.Exists(rootPath))
            {
                throw new InvalidOperationException($"Root directory {rootPath} does not exist.");
            }

            _root = rootPath;
        }

        protected override async Task<FileSet?> ExecuteItemAsync(Job job, string input)
        {
            var fileSet = await _fileScanner.ScanSourceAsync(input);
            if (fileSet != null && fileSet.Files.Count == 0)
            {
                _logger.LogInformation($"Project {fileSet.Name} contains no items after filtering.");
                return null;
            }

            return fileSet;
        }

        private IEnumerable<string> EvaluatePattern(string pattern)
        {
            var matchedFiles = Glob.Files(_root, pattern)
                                   .Select(path => Path.Combine(_root, path))
                                   .ToList();
            _logger.LogInformation($"Pattern {pattern} located {matchedFiles.Count} files.");
            return matchedFiles;
        }
    }
}