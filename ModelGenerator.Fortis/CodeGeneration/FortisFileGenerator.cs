using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Fortis.Configuration;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FortisFileGenerator : IGenerator<ModelFile>
    {
        private readonly IGenerator<ModelClass, MemberDeclarationSyntax> _classGenerator;
        private readonly IGenerator<ModelIdType, MemberDeclarationSyntax> _idGenerator;
        private readonly IGenerator<ModelInterface, MemberDeclarationSyntax> _interfaceGenerator;
        private readonly FortisSettings _settings;

        public FortisFileGenerator(
            FortisSettings settings,
            IGenerator<ModelClass, MemberDeclarationSyntax> classGenerator,
            IGenerator<ModelIdType, MemberDeclarationSyntax> idGenerator,
            IGenerator<ModelInterface, MemberDeclarationSyntax> interfaceGenerator)
        {
            _settings = settings;
            _classGenerator = classGenerator;
            _idGenerator = idGenerator;
            _interfaceGenerator = interfaceGenerator;
        }

        public IEnumerable<SyntaxNode> GenerateCode(GenerationContext context, ModelFile model)
        {
            yield return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(model.Namespace))
                                      .WithLeadingTrivia(SyntaxFactory.Comment("// Generated\n\n"))
                                      .AddUsings(GenerateUsings(context, model))
                                      .AddMembers(GenerateTypes(context, model));
        }

        private UsingDirectiveSyntax[] GenerateUsings(GenerationContext context, ModelFile model)
        {
            return _settings.NamespaceImports
                            .Select(ns => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns)))
                            .ToArray();
        }

        private MemberDeclarationSyntax[] GenerateTypes(GenerationContext context, ModelFile model)
        {
            return Enumerable.Empty<MemberDeclarationSyntax>()
                             .Union(model.Types
                                         .OfType<ModelInterface>()
                                         .SelectMany(model1 => _interfaceGenerator.GenerateCode(context, model1)))
                             .Union(model.Types
                                         .OfType<ModelClass>()
                                         .SelectMany(model1 => _classGenerator.GenerateCode(context, model1)))
                             .ToArray();
        }
    }
}