using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FortisFileGenerator : IGenerator<ModelFile>
    {
        private readonly IGenerator<ModelClass> _classGenerator;
        private readonly IGenerator<ModelIdType> _idGenerator;
        private readonly IGenerator<ModelInterface> _interfaceGenerator;

        public FortisFileGenerator(
            IGenerator<ModelClass> classGenerator,
            IGenerator<ModelIdType> idGenerator,
            IGenerator<ModelInterface> interfaceGenerator)
        {
            _classGenerator = classGenerator;
            _idGenerator = idGenerator;
            _interfaceGenerator = interfaceGenerator;
        }

        public IEnumerable<SyntaxNode> GenerateCode(GenerationContext context, ModelFile model)
        {
            yield return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(model.Namespace))
                                      .AddMembers(GenerateTypes(context, model));

        }

        private MemberDeclarationSyntax[] GenerateTypes(GenerationContext context, ModelFile model)
        {
            return Enumerable.Empty<MemberDeclarationSyntax>()
                             .Union(model.Types
                                         .OfType<ModelInterface>()
                                         .SelectMany(model1 => _interfaceGenerator.GenerateCode(context, model1))
                                         .Cast<MemberDeclarationSyntax>())
                             .ToArray();
        }
    }
}