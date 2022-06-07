using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Fortis.Configuration;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.TypeConstruction;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static ModelGenerator.Framework.CodeGeneration.SyntaxHelper;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public abstract class FortisIdGeneratorBase : FortisTypeGeneratorBase
    {
        private readonly FortisSettings _settings;

        public FortisIdGeneratorBase(FortisSettings settings, TypeNameResolver typeNameResolver)
            : base(settings, typeNameResolver)
        {
            _settings = settings;
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
                       ? base.GetNamespace(context, modelType)
                       : context.TypeSet.Namespace;
        }
    }
}