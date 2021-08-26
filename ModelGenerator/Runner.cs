using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Framework.FileScanning;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator
{
    public class Runner
    {
        private readonly IOptions<Settings> _settings;
        private readonly IFileScanner _fileScanner;
        private readonly IFileParser _fileParser;
        private readonly IDatabaseFactory _databaseFactory;
        private readonly ILogger<Runner> _logger;
        private readonly ITemplateCollectionFactory _templateFactory;

        public Runner(IOptions<Settings> settings,
            IFileScanner fileScanner,
            IFileParser fileParser,
            IDatabaseFactory databaseFactory,
            ILogger<Runner> logger,
            ITemplateCollectionFactory templateFactory)
        {
            _settings = settings;
            _fileScanner = fileScanner;
            _fileParser = fileParser;
            _databaseFactory = databaseFactory;
            _logger = logger;
            _templateFactory = templateFactory;
        }
        
        public async Task RunAsync(CancellationToken stoppingToken)
        {
            var itemSets = await GetRawItems(stoppingToken)
                                 .Where(f => f != null)
                                 .SelectAwait(ParseFilesInFileSet)
                                 .ToArrayAsync();

            var database = _databaseFactory.CreateDatabase(itemSets);
            var templates = _templateFactory.ConstructTemplates(database);
            foreach (var template in templates.Templates)
            {
                _logger.LogInformation(template.Value.Name);
            }
        }

        private async ValueTask<ItemSet> ParseFilesInFileSet(FileSet fileSet)
        {
            try
            {
                _logger.LogDebug($"Found {fileSet.Files.Count} files in {fileSet.Name}");
                var items = await fileSet.Files
                                         .ToAsyncEnumerable()
                                         .SelectMany(f => _fileParser.ParseFile(f))
                                         .ToDictionaryAsync(i => i.Id, i => i);

                return new ItemSet
                {
                    Id = fileSet.Id,
                    Name = fileSet.Name,
                    ItemPath = fileSet.ItemPath,
                    ModelPath = fileSet.ModelPath,
                    Items = items.ToImmutableDictionary()
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not parse files in fileset {fileSet.Name}");
                throw;
            }
        }

        private async IAsyncEnumerable<FileSet> GetRawItems(CancellationToken stoppingToken)
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

            foreach (var pattern in patterns)
            {
                var files = _fileScanner.FindFilesInPath(root, pattern);
                await foreach (var file in files.Where(f => f != null).WithCancellation(stoppingToken))
                {
                    if (file.Files.Count == 0)
                    {
                        _logger.LogInformation($"Project {file.Name} contains no items after filtering.");
                        continue;
                    }
                    
                    yield return file;
                }
            }
        }

        private IEnumerable<ItemSet> GetParsedItems(IEnumerable<FileSet> fileSets)
        {
            throw new NotImplementedException();
        }
    }
}