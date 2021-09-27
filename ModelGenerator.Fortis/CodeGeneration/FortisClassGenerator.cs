using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FortisClassGenerator : IGenerator<ModelClass, MemberDeclarationSyntax>
    {
        public IEnumerable<MemberDeclarationSyntax> GenerateCode(GenerationContext context, ModelClass model)
        {
            throw new System.NotImplementedException();
        }
    }
}