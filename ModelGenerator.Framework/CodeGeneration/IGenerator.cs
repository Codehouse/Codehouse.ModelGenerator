using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ModelGenerator.Framework.CodeGeneration
{
    public interface IGenerator<T> : IGenerator<T, SyntaxNode>
    {
    }

    public interface IGenerator<TModel, TNode>
    {
        IEnumerable<TNode> GenerateCode(GenerationContext context, TModel model);
    }
}