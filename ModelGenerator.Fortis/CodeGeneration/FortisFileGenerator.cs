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
        private readonly IGenerator<ModelClass, MemberDeclarationSyntax> _classGenerator;
        private readonly IGenerator<ModelIdType, MemberDeclarationSyntax> _idGenerator;
        private readonly IGenerator<ModelInterface, MemberDeclarationSyntax> _interfaceGenerator;

        public FortisFileGenerator(
            IGenerator<ModelClass, MemberDeclarationSyntax> classGenerator,
            IGenerator<ModelIdType, MemberDeclarationSyntax> idGenerator,
            IGenerator<ModelInterface, MemberDeclarationSyntax> interfaceGenerator)
        {
            _classGenerator = classGenerator;
            _idGenerator = idGenerator;
            _interfaceGenerator = interfaceGenerator;
        }

        public IEnumerable<SyntaxNode> GenerateCode(GenerationContext context, ModelFile model)
        {
            yield return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(model.Namespace))
                                      .WithLeadingTrivia(SyntaxFactory.Comment("// Generated\n\n"))
                                      .AddMembers(GenerateTypes(context, model));

        }

        private MemberDeclarationSyntax[] GenerateTypes(GenerationContext context, ModelFile model)
        {
            return Enumerable.Empty<MemberDeclarationSyntax>()
                             .Union(model.Types
                                         .OfType<ModelInterface>()
                                         .SelectMany(model1 => _interfaceGenerator.GenerateCode(context, model1)))
                             .ToArray();
        }
    }
}