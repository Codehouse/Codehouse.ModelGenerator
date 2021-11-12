using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.Activities
{
    public class GenerationActivity : CollectionActivityBase<GenerationContext, bool>
    {
        public override string Description => "Generate code";
        private readonly ICodeGenerator _codeGenerator;
        private readonly ILogger<GenerationActivity> _logger;

        public GenerationActivity(ILogger<GenerationActivity> logger, ICodeGenerator codeGenerator)
        {
            _logger = logger;
            _codeGenerator = codeGenerator;
        }

        protected override async Task<bool> ExecuteItemAsync(Job job, GenerationContext input)
        {
            var typeSet = input.TypeSet;

            _logger.LogInformation($"Generating files for {typeSet.Name} ({typeSet.Files.Count})");
            var tasks = typeSet.Files.Select(f => Task.Run(() => GenerateFile(input, f)));
            var generatedFiles = (await Task.WhenAll(tasks))
                                 .WhereNotNull()
                                 .ToList();

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

            return true;
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
    }
}