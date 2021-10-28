using System.IO;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration
{
    public interface ICodeGenerator
    {
        FileInfo? GenerateFile(GenerationContext context, ModelFile modelFile);
    }
}