using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ModelGenerator.Framework.CodeGeneration
{
    public record NamespacedType(
        string Namespace,
        TypeDeclarationSyntax Type,
        string TypeName);
}