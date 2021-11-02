using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelGenerator.Framework.Activities;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.Configuration;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Framework.FileScanning;
using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator
{
    public class Runner
    {
        // TODO: Separate out into individual classes.
        private readonly ICodeGenerator _codeGenerator;
        private readonly IDatabaseFactory _databaseFactory;
        private readonly IFileParser _fileParser;
        private readonly ILogger<Runner> _logger;
        private readonly IProgressTracker _progressTracker;
        private readonly IOptions<Settings> _settings;
        private readonly ProgressStep<FileScanActivity> _fileScanActivity;
        private readonly ITemplateCollectionFactory _templateFactory;
        private readonly ITypeFactory _typeFactory;

        public Runner(
            IOptions<Settings> settings,
            ProgressStep<FileScanActivity> fileScanActivity,
            ICodeGenerator codeGenerator,
            IFileParser fileParser,
            IDatabaseFactory databaseFactory,
            ILogger<Runner> logger,
            IProgressTracker progressTracker,
            ITemplateCollectionFactory templateFactory,
            ITypeFactory typeFactory)
        {
            _settings = settings;
            _fileScanActivity = fileScanActivity;
            _codeGenerator = codeGenerator;
            _fileParser = fileParser;
            _databaseFactory = databaseFactory;
            _logger = logger;
            _progressTracker = progressTracker;
            _templateFactory = templateFactory;
            _typeFactory = typeFactory;
        }

        public async Task RunAsync(CancellationToken stoppingToken)
        {
            // TODO: Ditch AsyncEnumerable
            // TODO: Add progress reporting/indicators
            using var job = _progressTracker.CreateJob("Starting up");

            var fileSets = await GetFileSets(job, stoppingToken);
            //var itemSets = await GetItemSets(job, fileSets, stoppingToken);

            var itemSets = await fileSets
                           .ToAsyncEnumerable()
                           .SelectAwait(ParseFilesInFileSet)
                           .ToArrayAsync();

            _logger.LogInformation("Constructing database.");
            var database = _databaseFactory.CreateDatabase(itemSets);
            
            _logger.LogInformation("Constructing template structure.");
            var templates = _templateFactory.ConstructTemplates(database);
            
            _logger.LogInformation("Constructing type sets.");
            var typeSets = _typeFactory.CreateTypeSets(templates);

            _logger.LogInformation("Outputting types.");
            foreach (var typeSet in typeSets)
            {
                var generatedFiles = new List<FileInfo>();
                var context = new GenerationContext
                {
                    Database = database,
                    Templates = templates,
                    TypeSet = typeSet
                };

                _logger.LogInformation($"Generating files for {typeSet.Name} ({typeSet.Files.Count})");
                foreach (var modelFile in typeSet.Files)
                {
                    var file = GenerateFile(context, modelFile);
                    if (file != null)
                    {
                        generatedFiles.Add(file);
                    }
                }

                var oldFiles = Directory.GetFiles(typeSet.RootPath, "*.cs", SearchOption.AllDirectories)
                         .Except(generatedFiles.Select(f => f.FullName))
                         .ToArray();
                if (oldFiles.Length > 0)
                {
                    _logger.LogInformation($"Cleaning up {oldFiles.Length} files.");
                    foreach (var oldFile in oldFiles)
                    {
                        File.Delete(oldFile);
                    }
                }
            }
        }

        private async Task<ICollection<ItemSet>> GetItemSets(Job job, ICollection<FileSet> fileSets, CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }

        private async Task<ICollection<FileSet>> GetFileSets(Job job, CancellationToken stoppingToken)
        {
            // TODO: Make TDS settings TDS-specific.
            job.Description = "Locating files";
            _logger.LogInformation("Locating item files.");
            _fileScanActivity.Activity.SetRoot(_settings.Value.Root);
            _fileScanActivity.Activity.SetInput(_settings.Value.Patterns);
            await _fileScanActivity.ExecuteAsync(stoppingToken);
            return _fileScanActivity.Activity.GetOutput();
        }

        private FileInfo? GenerateFile(GenerationContext context, ModelFile modelFile)
        {
            try
            {
                return _codeGenerator.GenerateFile(context, modelFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not generate file {modelFile.FileName}");
                return null;
            }
        }

        private async ValueTask<ItemSet> ParseFilesInFileSet(FileSet fileSet)
        {
            try
            {
                _logger.LogDebug($"Found {fileSet.Files.Count} files in {fileSet.Name}");
                var items = await fileSet.Files
                                         .ToAsyncEnumerable()
                                         .SelectMany(f => _fileParser.ParseFile(fileSet, f))
                                         .ToDictionaryAsync(i => i.Id, i => i);

                return new ItemSet
                {
                    Id = fileSet.Id,
                    Name = fileSet.Name,
                    Namespace = fileSet.Namespace,
                    ItemPath = fileSet.ItemPath,
                    ModelPath = fileSet.ModelPath,
                    References = fileSet.References,
                    Items = items.ToImmutableDictionary()
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not parse files in fileset {fileSet.Name}");
                throw;
            }
        }
    }
}