using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Fortis.Configuration;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FortisFileGenerator : IFileGenerator
    {
        private readonly FortisClassGenerator _classGenerator;
        private readonly FortisIdGenerator _idGenerator;
        private readonly FortisInterfaceGenerator _interfaceGenerator;
        private readonly FortisSettings _settings;
        private readonly TypeNameResolver _typeNameResolver;

        public FortisFileGenerator(
            FortisSettings settings,
            FortisClassGenerator classGenerator,
            FortisIdGenerator idGenerator,
            FortisInterfaceGenerator interfaceGenerator,
            TypeNameResolver typeNameResolver)
        {
            _settings = settings;
            _classGenerator = classGenerator;
            _idGenerator = idGenerator;
            _interfaceGenerator = interfaceGenerator;
            _typeNameResolver = typeNameResolver;
        }

        public CompilationUnitSyntax GenerateCode(ScopedRagBuilder<string> ragBuilder, GenerationContext context, ModelFile model)
        {
            return CompilationUnit()
                   .AddUsings(GenerateUsings())
                   .AddMembers(GenerateNamespaces(ragBuilder, context, model).ToArray());
        }

        private IEnumerable<NamespaceDeclarationSyntax> GenerateNamespaces(ScopedRagBuilder<string> ragBuilder, GenerationContext context, ModelFile model)
        {
            var namespaceTypeGroups = model.Types
                                           .GroupBy(t => _typeNameResolver.GetNamespace(context.TypeSet, t.Template));
            foreach (var namespaceTypeGroup in namespaceTypeGroups)
            {
                var members = namespaceTypeGroup
                              .SelectMany(t => GenerateTypes(ragBuilder, context, t))
                              .ToArray();

                yield return NamespaceDeclaration(ParseName(namespaceTypeGroup.Key))
                             .WithLeadingTrivia(Comment("// Generated"), EndOfLine(string.Empty))
                             .AddMembers(members);
            }

            if (!_settings.Quirks.LocalNamespaceForIds)
            {
                yield return NamespaceDeclaration(ParseName(context.TypeSet.Namespace))
                    .AddMembers(_idGenerator.GenerateCode(ragBuilder, context, model.Types).ToArray());
            }
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateTypes(ScopedRagBuilder<string> ragBuilder, GenerationContext context, ModelType model)
        {
            var types = _interfaceGenerator.GenerateCode(ragBuilder, context, model)
                                           .Union(_classGenerator.GenerateCode(ragBuilder, context, model));
            if (_settings.Quirks.LocalNamespaceForIds)
            {
                types = types.Union(_idGenerator.GenerateCode(ragBuilder, context, model));
            }

            return types;
        }

        private UsingDirectiveSyntax[] GenerateUsings()
        {
            // TODO: Add support for namespace and type aliasing
            return _settings.NamespaceImports
                            .Select(ns => UsingDirective(ParseName(ns)))
                            .ToArray();
        }
    }
}