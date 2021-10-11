using System.IO;
using Microsoft.CodeAnalysis;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration
{
    public class CodeGenerator : ICodeGenerator
    {
        private readonly IGenerator<ModelFile> _fileGenerator;
        
        public CodeGenerator(IGenerator<ModelFile> fileGenerator)
        {
            _fileGenerator = fileGenerator;
        }

        public void GenerateFile(GenerationContext context, string modelFolder, ModelFile modelFile)
        {
            var syntaxNodes = _fileGenerator.GenerateCode(context, modelFile);
            
            var directory = Path.Combine(modelFile.RootPath, modelFolder);
            var filePath = Path.Combine(directory, modelFile.FileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var file = new StreamWriter(filePath, false);
            foreach (var syntaxNode in syntaxNodes)
            {
                syntaxNode.NormalizeWhitespace()
                          .WriteTo(file);
            }

            file.Flush();
        }
    }
}