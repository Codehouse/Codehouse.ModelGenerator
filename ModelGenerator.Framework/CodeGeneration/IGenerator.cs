using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration
{
    public interface IFileGenerator
    {
        CompilationUnitSyntax GenerateCode(GenerationContext context, ModelFile model);
    }
}