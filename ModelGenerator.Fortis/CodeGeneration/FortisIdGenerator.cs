using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FortisIdGenerator : IGenerator<ModelIdType, MemberDeclarationSyntax>
    {
        public IEnumerable<MemberDeclarationSyntax> GenerateCode(GenerationContext context, ModelIdType model)
        {
            throw new NotImplementedException();
        }
    }
}