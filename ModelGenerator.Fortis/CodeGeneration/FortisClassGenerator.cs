using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FortisClassGenerator : IGenerator<ModelClass>
    {
        public IEnumerable<SyntaxNode> GenerateCode(GenerationContext context, ModelClass model)
        {
            throw new System.NotImplementedException();
        }
    }
}