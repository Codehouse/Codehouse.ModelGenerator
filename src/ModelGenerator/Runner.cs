﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
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
    /// <summary>
    /// This class handles the overall orchestration of the model generator activities
    /// and processes.
    /// <para>
    ///     This includes doing various precondition checks (e.g. version), managing
    ///     the progress indicators, chaining the various activities, and displaying
    ///     any necessary output.
    /// </para>
    /// </summary>
    public class Runner
    {
        private readonly ProgressStep<DatabaseActivity> _databaseActivity;
        private readonly ProgressStep<FileParseActivity> _fileParseActivity;
        private readonly ProgressStep<FileScanActivity> _fileScanActivity;
        private readonly ProgressStep<GenerationActivity> _generationActivity;
        private readonly ILogger<Runner> _logger;
        private readonly Lazy<IProgressTracker> _progressTrackerFactory;
        private readonly IOptions<Settings> _settings;
        private readonly ISourceProvider _sourceProvider;
        private readonly ProgressStep<TemplateActivity> _templateActivity;
        private readonly ProgressStep<TypeActivity> _typeActivity;

        public Runner(
            IOptions<Settings> settings,
            ISourceProvider sourceProvider,
            ProgressStep<DatabaseActivity> databaseActivity,
            ProgressStep<FileParseActivity> fileParseActivity,
            ProgressStep<FileScanActivity> fileScanActivity,
            ProgressStep<GenerationActivity> generationActivity,
            ProgressStep<TemplateActivity> templateActivity,
            ProgressStep<TypeActivity> typeActivity,
            ILogger<Runner> logger,
            Lazy<IProgressTracker> progressTrackerFactory)
        {
            _settings = settings;
            _sourceProvider = sourceProvider;
            _databaseActivity = databaseActivity;
            _fileParseActivity = fileParseActivity;
            _fileScanActivity = fileScanActivity;
            _generationActivity = generationActivity;
            _templateActivity = templateActivity;
            _typeActivity = typeActivity;
            _logger = logger;
            _progressTrackerFactory = progressTrackerFactory;
        }

        /// <summary>
        /// Runs the runner, with the given <paramref name="stoppingToken"/>
        /// allowing the process to be aborted.
        /// </summary>
        /// <param name="stoppingToken">A cancellation token</param>
        public async Task RunAsync(CancellationToken stoppingToken)
        {
            if (!CheckVersion())
            {
                return;
            }

            using var progressTracker = _progressTrackerFactory.Value;
            using var job = progressTracker.CreateJob("Overall progress");
            job.MaxValue = 6;
            job.Start();

            var fileSetReport = await GetFileSets(job, stoppingToken);
            var itemSetReport = await GetItemSets(job, fileSetReport.Result, stoppingToken);
            var databaseReport = await ConstructDatabase(job, itemSetReport.Result, stoppingToken);
            var templateReport = await ConstructTemplates(job, databaseReport.Result, stoppingToken);
            var typeSetReport = await CreateTypeSets(job, templateReport.Result, stoppingToken);
            var generationReport = await GenerateCode(job, templateReport.Result, typeSetReport.Result, stoppingToken);

            job.Stop();
            progressTracker.Finish();

            GC.Collect(3);
            PrintReports(fileSetReport, itemSetReport, databaseReport, templateReport, typeSetReport, generationReport);
        }

        /// <summary>
        /// Checks whether the application version matches the minimum version
        /// required by the application configuration.
        /// </summary>
        /// <returns>True if the version matches, false otherwise</returns>
        /// <exception cref="InvalidOperationException">If the entry assembly cannot be resolved.</exception>
        private bool CheckVersion()
        {
            var currentVersion = Assembly.GetEntryAssembly()?.GetName().Version
                              ?? throw new InvalidOperationException("Could not identify application version.");

            if (string.IsNullOrEmpty(_settings.Value.MinVersion))
            {
                var message = $"Minimum version not set in the config, set it to {currentVersion.ToString(2)}.";
                _logger.LogInformation(message);
                Console.WriteLine(message);
                return false;
            }

            var minVersion = Version.Parse(_settings.Value.MinVersion);
            if (minVersion > currentVersion)
            {
                var message = $"You are using {currentVersion} of the model generator tool, and you must be using at least {minVersion}.";
                _logger.LogCritical(message);
                Console.WriteLine(message);
                return false;
            }

            if (currentVersion.Major > minVersion.Major || currentVersion.Minor > minVersion.Minor)
            {
                var message = $"You are using {currentVersion} of the model generator tool, which may not be compatible with {minVersion}.";
                _logger.LogError(message);
                Console.WriteLine(message);

                message = $"Update the min version to {currentVersion.ToString(2)}.";
                _logger.LogError(message);
                Console.WriteLine(message);

                return false;
            }

            return true;
        }

        private Task<IReport<IDatabase>> ConstructDatabase(Job job, ICollection<ItemSet> itemSets, CancellationToken stoppingToken)
        {
            return RunStep<DatabaseActivity, IDatabase, ICollection<ItemSet>>(job, _databaseActivity, itemSets, stoppingToken);
        }

        private Task<IReport<TemplateCollection>> ConstructTemplates(Job job, IDatabase database, CancellationToken stoppingToken)
        {
            return RunStep<TemplateActivity, TemplateCollection, IDatabase>(job, _templateActivity, database, stoppingToken);
        }

        private Task<IReport<IImmutableList<TypeSet>>> CreateTypeSets(Job job, TemplateCollection templates, CancellationToken stoppingToken)
        {
            return RunStep<TypeActivity, IImmutableList<TypeSet>, TemplateCollection>(job, _typeActivity, templates, stoppingToken);
        }

        private Task<IReport<ICollection<FileInfo>>> GenerateCode(Job job, TemplateCollection templates, IEnumerable<TypeSet> typeSets, CancellationToken stoppingToken)
        {
            var contexts = typeSets.Select(t => new GenerationContext(templates, t));

            return RunStep<GenerationActivity, ICollection<FileInfo>, IEnumerable<GenerationContext>>(job, _generationActivity, contexts, stoppingToken);
        }

        private Task<IReport<ICollection<FileSet>>> GetFileSets(Job job, CancellationToken stoppingToken)
        {
            var sources = _sourceProvider.GetSources();
            return RunStep<FileScanActivity, ICollection<FileSet>, IEnumerable<string>>(job, _fileScanActivity, sources, stoppingToken);
        }

        private Task<IReport<ICollection<ItemSet>>> GetItemSets(Job job, ICollection<FileSet> fileSets, CancellationToken stoppingToken)
        {
            return RunStep<FileParseActivity, ICollection<ItemSet>, IEnumerable<FileSet>>(job, _fileParseActivity, fileSets, stoppingToken);
        }

        private void PrintReports(params IReport[] reports)
        {
            foreach (var report in reports)
            {
                report.Print(_settings.Value.Verbosity);
            }
        }

        /// <summary>
        /// Runs the step for a given <typeparamref name="TActivity"/> activity
        /// asynchronously, and returns the result wrapped inside a report object.
        /// </summary>
        /// <param name="overallJob">The "overall progress" job</param>
        /// <param name="step">A <see cref="ProgressStep{T}"/> type referencing the desired <typeparamref name="TActivity"/></param>
        /// <param name="input">The activity input</param>
        /// <param name="stoppingToken">A cancellation token</param>
        /// <typeparam name="TActivity">The activity type</typeparam>
        /// <typeparam name="TResult">The activity output type</typeparam>
        /// <typeparam name="TInput">The activity input type</typeparam>
        /// <returns>The activity result wrapped inside a report object.</returns>
        /// <exception cref="InvalidOperationException">If the step activity's output is <c>null</c></exception>
        private async Task<IReport<TResult>> RunStep<TActivity, TResult, TInput>(Job overallJob, ProgressStep<TActivity> step, TInput input, CancellationToken stoppingToken)
            where TActivity : IActivity<TInput, TResult>
        {
            _logger.LogInformation(step.Activity.Description);
            step.Activity.SetInput(input);

            await step.ExecuteAsync(_progressTrackerFactory.Value, stoppingToken);
            overallJob.Increment();

            var report = step.Activity.GetOutput();
            if (report.Result == null)
            {
                _logger.LogWarning($"Result of step '{step.Activity.Description}' was null.");
                throw new InvalidOperationException("Step result was null.");
            }

            return report;
        }
    }
}