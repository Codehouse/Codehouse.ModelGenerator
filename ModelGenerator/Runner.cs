using System.Collections.Generic;
using System.Collections.Immutable;
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
        private readonly ProgressStep<DatabaseActivity> _databaseActivity;
        private readonly ProgressStep<FileParseActivity> _fileParseActivity;
        private readonly ProgressStep<FileScanActivity> _fileScanActivity;
        private readonly ProgressStep<GenerationActivity> _generationActivity;
        private readonly ILogger<Runner> _logger;
        private readonly IProgressTracker _progressTracker;
        private readonly IOptions<Settings> _settings;
        private readonly ProgressStep<TemplateActivity> _templateActivity;
        private readonly ProgressStep<TypeActivity> _typeActivity;

        public Runner(
            IOptions<Settings> settings,
            ProgressStep<DatabaseActivity> databaseActivity,
            ProgressStep<FileParseActivity> fileParseActivity,
            ProgressStep<FileScanActivity> fileScanActivity,
            ProgressStep<GenerationActivity> generationActivity,
            ProgressStep<TemplateActivity> templateActivity,
            ProgressStep<TypeActivity> typeActivity,
            ILogger<Runner> logger,
            IProgressTracker progressTracker)
        {
            _settings = settings;
            _databaseActivity = databaseActivity;
            _fileParseActivity = fileParseActivity;
            _fileScanActivity = fileScanActivity;
            _generationActivity = generationActivity;
            _templateActivity = templateActivity;
            _typeActivity = typeActivity;
            _logger = logger;
            _progressTracker = progressTracker;
        }

        public async Task RunAsync(CancellationToken stoppingToken)
        {
            // TODO: Add progress reporting/indicators
            using var job = _progressTracker.CreateJob("Overall progress");
            job.MaxValue = 6;

            var fileSets = await GetFileSets(job, stoppingToken);
            var itemSets = await GetItemSets(job, fileSets, stoppingToken);
            var database = await ConstructDatabase(job, itemSets, stoppingToken);
            var templates = await ConstructTemplates(job, database, stoppingToken);
            var typeSets = await CreateTypeSets(job, templates, stoppingToken);

            await GenerateCode(job, database, templates, typeSets, stoppingToken);
            job.Stop();
        }

        private async Task<IDatabase> ConstructDatabase(Job job, ICollection<ItemSet> itemSets, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Constructing database.");
            _databaseActivity.Activity.SetInput(itemSets);
            await _databaseActivity.ExecuteAsync(stoppingToken);

            job.Increment();
            return _databaseActivity.Activity.GetOutput();
        }

        private async Task<TemplateCollection> ConstructTemplates(Job job, IDatabase database, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Constructing template structure.");
            _templateActivity.Activity.SetInput(database);
            await _templateActivity.ExecuteAsync(stoppingToken);

            job.Increment();
            return _templateActivity.Activity.GetOutput();
        }

        private async Task<IImmutableList<TypeSet>> CreateTypeSets(Job job, TemplateCollection templates, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Constructing type sets.");
            _typeActivity.Activity.SetInput(templates);
            await _typeActivity.ExecuteAsync(cancellationToken);

            job.Increment();
            return _typeActivity.Activity.GetOutput();
        }

        private async Task GenerateCode(Job job, IDatabase database, TemplateCollection templates, IEnumerable<TypeSet> typeSets, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outputting types.");
            var contexts = typeSets.Select(t => new GenerationContext
            {
                Database = database,
                Templates = templates,
                TypeSet = t
            });

            _generationActivity.Activity.SetInput(contexts);
            await _generationActivity.ExecuteAsync(stoppingToken);
            job.Increment();
        }

        private async Task<ICollection<FileSet>> GetFileSets(Job job, CancellationToken stoppingToken)
        {
            // TODO: Make TDS settings TDS-specific.
            _logger.LogInformation("Locating item files.");

            _fileScanActivity.Activity.SetRoot(_settings.Value.Root);
            _fileScanActivity.Activity.SetInput(_settings.Value.Patterns);
            await _fileScanActivity.ExecuteAsync(stoppingToken);

            job.Increment();
            return _fileScanActivity.Activity.GetOutput();
        }

        private async Task<ICollection<ItemSet>> GetItemSets(Job job, ICollection<FileSet> fileSets, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Parsing item files.");

            _fileParseActivity.Activity.SetInput(fileSets);
            await _fileParseActivity.ExecuteAsync(stoppingToken);

            job.Increment();
            return _fileParseActivity.Activity.GetOutput();
        }
    }
}