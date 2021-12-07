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

        private readonly RagBuilder<string> _builder = new();
        private readonly IFileScanner _fileScanner;
        private readonly ILogger<FileScanActivity> _logger;

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

            base.SetInput(input);
        }

        protected override IReport<ICollection<FileSet>> CreateReport(ICollection<FileSet> results)
        {
            return new RagReport<ICollection<FileSet>, string>(Description, _builder, results);
        }

        protected override async Task<FileSet?> ExecuteItemAsync(Job job, string input)
        {
            var fileSet = await _fileScanner.ScanSourceAsync(_builder, input);
            if (fileSet != null && fileSet.Files.Count == 0)
            {
                _logger.LogInformation($"Project {fileSet.Name} contains no items after filtering.");
                return null;
            }

            return fileSet;
        }

    }
}