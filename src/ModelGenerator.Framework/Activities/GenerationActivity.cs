using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.CodeGeneration.FileTypes;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.Activities
{
    public class GenerationActivity : CollectionActivityBase<GenerationContext, FileInfo, FileInfo[]>
    {
        public override string Description => "Generate code";

        private readonly IFileFactory[] _fileFactories;
        private readonly IFileGenerator[] _fileGenerators;
        private readonly ILogger<GenerationActivity> _logger;
        private readonly RagBuilder<string> _ragBuilder = new();

        public GenerationActivity(
            ILogger<GenerationActivity> logger,
            IEnumerable<IFileFactory> fileFactories,
            IEnumerable<IFileGenerator> fileGenerators)
        {
            _logger = logger;
            _fileFactories = fileFactories.ToArray();
            _fileGenerators = fileGenerators.ToArray();
        }

        protected override ICollection<FileInfo> ConvertResults(FileInfo?[]?[] results)
        {
            return results.WhereNotNull()
                          .SelectMany(s => s)
                          .WhereNotNull()
                          .ToArray();
        }

        protected override IReport<ICollection<FileInfo>> CreateReport(ICollection<FileInfo> results)
        {
            return new RagReport<ICollection<FileInfo>, string>(Description,
                _ragBuilder,
                results);
        }

        protected override async Task<FileInfo[]?> ExecuteItemAsync(Job job, GenerationContext input)
        {
            var typeSet = input.TypeSet;

            _logger.LogInformation($"Generating files for {typeSet.Name} ({typeSet.Files.Count})");
            var tasks = typeSet
                       .Files
                       .Select(f => Task.Run(() => GenerateFiles(input, f)));
            var generatedFiles = (await Task.WhenAll(tasks))
                                .SelectMany(a => a)
                                .ToArray();

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

            return generatedFiles;
        }

        private FileInfo? GenerateFile(IFileType file)
        {
            var generator = _fileGenerators.Single(g => g.CanGenerate(file));
            try
            {
                var fileInfo = generator.GenerateFile(file);
                if (fileInfo == null)
                {
                    file.ScopedRagBuilder.AddFail("Did not generate a file.");
                }
                else if (file.ScopedRagBuilder.CanPass)
                {
                    _ragBuilder.AddPass(fileInfo.FullName);
                }

                return fileInfo;
            }
            catch (Exception ex)
            {
                file.ScopedRagBuilder.AddFail("Error while generating file.", ex);
                _logger.LogError(ex, $"Could not generate file {file.Model.FileName}");
                return null;
            }
        }

        private IEnumerable<FileInfo> GenerateFiles(GenerationContext context, ModelFile modelFile)
        {
            using var scopedRagBuilder = _ragBuilder.CreateScope($"{context.TypeSet.Name} - {modelFile.FileName}");
            return _fileFactories.Select(f => f.CreateFile(context, modelFile, scopedRagBuilder))
                                 .Select(GenerateFile)
                                 .WhereNotNull()
                                 .ToArray();
        }
    }
}