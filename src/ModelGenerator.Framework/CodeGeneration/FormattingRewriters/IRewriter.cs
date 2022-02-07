using Microsoft.CodeAnalysis;

namespace ModelGenerator.Framework.CodeGeneration
{
    public interface IRewriter
    {
        SyntaxNode? Visit(SyntaxNode? node);
    }
}