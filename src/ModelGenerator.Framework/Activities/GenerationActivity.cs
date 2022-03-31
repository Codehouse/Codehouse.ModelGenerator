using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.Activities
{
    public class GenerationActivity : CollectionActivityBase<GenerationContext, FileInfo, FileInfo[]>
    {
        public override string Description => "Generate code";
        private readonly ICodeGenerator _codeGenerator;
        private readonly ILogger<GenerationActivity> _logger;
        private readonly RagBuilder<string> _ragBuilder = new();

        public GenerationActivity(ILogger<GenerationActivity> logger, ICodeGenerator codeGenerator)
        {
            _logger = logger;
            _codeGenerator = codeGenerator;
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
            var tasks = typeSet.Files.Select(f => Task.Run(() => GenerateFile(input, f)));
            var generatedFiles = (await Task.WhenAll(tasks))
                                 .WhereNotNull()
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

        private FileInfo? GenerateFile(GenerationContext context, ModelFile modelFile)
        {
            using var scopedRagBuilder = _ragBuilder.CreateScope($"{context.TypeSet.Name} - {modelFile.FileName}");
            try
            {
                var file = _codeGenerator.GenerateFile(scopedRagBuilder, context, modelFile);
                if (file == null)
                {
                    scopedRagBuilder.AddFail("Did not generate a file.");
                }
                else if (scopedRagBuilder.CanPass)
                {
                    _ragBuilder.AddPass(file.FullName);
                }

                return file;
            }
            catch (Exception ex)
            {
                scopedRagBuilder.AddFail("Error while generating file.", ex);
                _logger.LogError(ex, $"Could not generate file {modelFile.FileName}");
                return null;
            }
        }
    }
}