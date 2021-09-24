using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FortisIdGenerator : IGenerator<ModelIdType>
    {
        public IEnumerable<SyntaxNode> GenerateCode(GenerationContext context, ModelIdType model)
        {
            throw new System.NotImplementedException();
        }
    }
}