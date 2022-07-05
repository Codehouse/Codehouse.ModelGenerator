using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.CodeGeneration.FileTypes;
using ModelGenerator.Framework.TypeConstruction;
using ModelGenerator.IdClasses.Configuration;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static ModelGenerator.Framework.CodeGeneration.SyntaxHelper;

namespace ModelGenerator.IdClasses.CodeGeneration
{
    public abstract class IdTypeGeneratorBase : TypeGeneratorBase<DefaultFile>
    {
        private readonly IdSettings _settings;
        private readonly ITypeNameResolver _typeNameResolver;

        protected IdTypeGeneratorBase(IdSettings settings, ITypeNameResolver typeNameResolver)
        {
            _settings = settings;
            _typeNameResolver = typeNameResolver;
        }

        protected PropertyDeclarationSyntax GenerateIdProperty(string name, Guid value)
        {
            return PropertyDeclaration(IdentifierName(nameof(Guid)), name)
                  .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                  .AddAccessorListAccessors(AutoGet())
                  .WithInitializer(
                       EqualsValueClause(
                           InvocationExpression(
                                   MemberAccessExpression(
                                       SyntaxKind.SimpleMemberAccessExpression,
                                       IdentifierName(nameof(Guid)),
                                       IdentifierName("Parse")
                                   )
                               )
                              .AddArgumentListArguments(Argument(IdLiteral(value)))
                       )
                   )
                  .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        protected override string GetNamespace(GenerationContext context, ModelType modelType)
        {
            return _settings.Quirks.LocalNamespaceForIds
                ? _typeNameResolver.GetNamespace(modelType.TemplateSet, modelType.Template)
                : context.TypeSet.Namespace;
        }
    }
}