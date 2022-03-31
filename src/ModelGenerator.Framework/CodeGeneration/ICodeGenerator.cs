using System.IO;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration
{
    public interface ICodeGenerator
    {
        FileInfo? GenerateFile(ScopedRagBuilder<string> ragBuilder, GenerationContext context, ModelFile modelFile);
    }
}