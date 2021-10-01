using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Fortis.Configuration;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.TypeConstruction;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
            yield return NamespaceDeclaration(ParseName(model.Namespace))
                                      .WithLeadingTrivia(Comment("// Generated"), EndOfLine(string.Empty))
                                      .AddUsings(GenerateUsings(context, model))
                                      .AddMembers(GenerateTypes(context, model));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateType(GenerationContext context, ModelType model)
        {
            return model switch
            {
                ModelClass type => _classGenerator.GenerateCode(context, type),
                ModelIdType type => _idGenerator.GenerateCode(context, type),
                ModelInterface type => _interfaceGenerator.GenerateCode(context, type),
                _ => throw new NotSupportedException($"Unknown model type: {model.GetType().Name}")
            };
        }

        private MemberDeclarationSyntax[] GenerateTypes(GenerationContext context, ModelFile model)
        {
            return model.Types
                        .SelectMany(t => GenerateType(context, t))
                        .ToArray();
        }

        private UsingDirectiveSyntax[] GenerateUsings(GenerationContext context, ModelFile model)
        {
            return _settings.NamespaceImports
                            .Select(ns => UsingDirective(ParseName(ns)))
                            .ToArray();
        }
    }
}