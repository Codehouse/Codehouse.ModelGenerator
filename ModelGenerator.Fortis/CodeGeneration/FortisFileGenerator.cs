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
        private readonly TypeNameResolver _typeNameResolver;
        private readonly FortisSettings _settings;

        public FortisFileGenerator(
            FortisSettings settings,
            IGenerator<ModelClass, MemberDeclarationSyntax> classGenerator,
            IGenerator<ModelIdType, MemberDeclarationSyntax> idGenerator,
            IGenerator<ModelInterface, MemberDeclarationSyntax> interfaceGenerator,
            TypeNameResolver typeNameResolver)
        {
            _settings = settings;
            _classGenerator = classGenerator;
            _idGenerator = idGenerator;
            _interfaceGenerator = interfaceGenerator;
            _typeNameResolver = typeNameResolver;
        }

        public IEnumerable<SyntaxNode> GenerateCode(GenerationContext context, ModelFile model)
        {
            yield return CompilationUnit()
                         .AddUsings(GenerateUsings(context))
                         .AddMembers(GenerateNamespaces(context, model).ToArray());
        }

        private IEnumerable<NamespaceDeclarationSyntax> GenerateNamespaces(GenerationContext context, ModelFile model)
        {
            var namespaceTypeGroups = model.Types
                                           .GroupBy(t => _typeNameResolver.GetNamespace(context.TypeSet, t.Template));
            foreach (var namespaceTypeGroup in namespaceTypeGroups)
            {
                yield return NamespaceDeclaration(ParseName(namespaceTypeGroup.Key))
                                          .WithLeadingTrivia(Comment("// Generated"), EndOfLine(string.Empty))
                                          .AddMembers(GenerateTypes(context, namespaceTypeGroup));
            }
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

        private MemberDeclarationSyntax[] GenerateTypes(GenerationContext context, IEnumerable<ModelType> modelTypes)
        {
            return modelTypes
                   .SelectMany(t => GenerateType(context, t))
                   .ToArray();
        }

        private UsingDirectiveSyntax[] GenerateUsings(GenerationContext context)
        {
            return _settings.NamespaceImports
                            .Select(ns => UsingDirective(ParseName(ns)))
                            .ToArray();
        }
    }
}