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
    public class FortisFileGenerator : IFileGenerator
    {
        private readonly FortisClassGenerator _classGenerator;
        private readonly FortisIdGenerator _idGenerator;
        private readonly FortisInterfaceGenerator _interfaceGenerator;
        private readonly TypeNameResolver _typeNameResolver;
        private readonly FortisSettings _settings;

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

        public CompilationUnitSyntax GenerateCode(GenerationContext context, ModelFile model)
        {
            return CompilationUnit()
                   .AddUsings(GenerateUsings())
                   .AddMembers(GenerateNamespaces(context, model).ToArray());
        }

        private IEnumerable<NamespaceDeclarationSyntax> GenerateNamespaces(GenerationContext context, ModelFile model)
        {
            var namespaceTypeGroups = model.Types
                                           .GroupBy(t => _typeNameResolver.GetNamespace(context.TypeSet, t.Template));
            foreach (var namespaceTypeGroup in namespaceTypeGroups)
            {
                var members = namespaceTypeGroup
                    .SelectMany(t => GenerateTypes(context, t))
                    .ToArray();
                
                yield return NamespaceDeclaration(ParseName(namespaceTypeGroup.Key))
                                          .WithLeadingTrivia(Comment("// Generated"), EndOfLine(string.Empty))
                                          .AddMembers(members);
            }

            if (!_settings.Quirks.LocalNamespaceForIds)
            {
                yield return NamespaceDeclaration(ParseName(context.TypeSet.Namespace))
                    .AddMembers(_idGenerator.GenerateCode(context, model.Types).ToArray());
            }
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateTypes(GenerationContext context, ModelType model)
        {
            var types = _interfaceGenerator.GenerateCode(context, model)
                                           .Union(_classGenerator.GenerateCode(context, model));
            if (_settings.Quirks.LocalNamespaceForIds)
            {
                types = types.Union(_idGenerator.GenerateCode(context, model));
            }

            return types;
        }

        private UsingDirectiveSyntax[] GenerateUsings()
        {
            return _settings.NamespaceImports
                            .Select(ns => UsingDirective(ParseName(ns)))
                            .ToArray();
        }
    }
}