using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ModelGenerator.Framework.CodeGeneration
{
    [DebuggerDisplay("Type: {Namespace}.{TypeName}")]
    public record NamespacedType(
        string Namespace,
        TypeDeclarationSyntax Type,
        string TypeName);
}