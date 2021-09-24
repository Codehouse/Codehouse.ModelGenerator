using System.IO;
using Microsoft.CodeAnalysis;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration
{
    public class CodeGenerator : ICodeGenerator
    {
        private readonly IGenerator<ModelFile> _fileGenerator;

        private const string _indentation = "    ";
        private const string _eol = "\r\n";

        public CodeGenerator(IGenerator<ModelFile> fileGenerator)
        {
            _fileGenerator = fileGenerator;
        }
        
        public void GenerateFile(GenerationContext context, ModelFile modelFile)
        {
            var syntaxNodes = _fileGenerator.GenerateCode(context, modelFile);

            using var file = new StreamWriter(Path.Combine(modelFile.Path, modelFile.FileName), false);
            foreach (var syntaxNode in syntaxNodes)
            {
                syntaxNode.NormalizeWhitespace(_indentation, _eol, false)
                          .WriteTo(file);
            }
            
            file.Flush();
        }
    }
}