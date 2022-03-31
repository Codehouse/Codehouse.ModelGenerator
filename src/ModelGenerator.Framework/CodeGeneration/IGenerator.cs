using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration
{
    public interface IFileGenerator
    {
        CompilationUnitSyntax GenerateCode(ScopedRagBuilder<string> scopedRagBuilder, GenerationContext context, ModelFile model);
    }
}