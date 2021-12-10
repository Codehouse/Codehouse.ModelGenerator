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

        public FileInfo? GenerateFile(GenerationContext context, ModelFile modelFile)
        {
            var syntax = GenerateCode(context, modelFile);
            if (syntax == null)
            {
                return null;
            }

            return WriteCode(modelFile, syntax);
        }

        private static FileInfo WriteCode(ModelFile modelFile, SyntaxNode syntax)
        {
            var filePath = Path.Combine(modelFile.RootPath, modelFile.FileName);
            if (!Directory.Exists(modelFile.RootPath))
            {
                Directory.CreateDirectory(modelFile.RootPath);
            }

            using var file = new StreamWriter(filePath, false);
            syntax.NormalizeWhitespace()
                  .WriteTo(file);

            file.Flush();
            return new FileInfo(filePath);
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
                throw;
            }
        }
    }
}