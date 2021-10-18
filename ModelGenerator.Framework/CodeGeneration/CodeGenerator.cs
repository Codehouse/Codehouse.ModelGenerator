using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration
{
    public class CodeGenerator : ICodeGenerator
    {
        private readonly IFileGenerator _fileGenerator;
        private readonly ILogger<CodeGenerator> _logger;

        public CodeGenerator(IFileGenerator fileGenerator, ILogger<CodeGenerator> logger)
        {
            _fileGenerator = fileGenerator;
            _logger = logger;
        }

        public void GenerateFile(GenerationContext context, string modelFolder, ModelFile modelFile)
        {
            var syntax = GenerateCode(context, modelFile);
            if (syntax == null)
            {
                return;
            }
            
            WriteCode(modelFolder, modelFile, syntax);
        }

        private static void WriteCode(string modelFolder, ModelFile modelFile, SyntaxNode syntax)
        {
            var directory = Path.Combine(modelFile.RootPath, modelFolder);
            var filePath = Path.Combine(directory, modelFile.FileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var file = new StreamWriter(filePath, false);
            syntax.NormalizeWhitespace()
                  .WriteTo(file);

            file.Flush();
        }

        private CompilationUnitSyntax? GenerateCode(GenerationContext context, ModelFile modelFile)
        {
            try
            {
                return _fileGenerator.GenerateCode(context, modelFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not generate code for file {modelFile.FileName} in {modelFile.RootPath}");
                return null;
            }
        }
    }
}