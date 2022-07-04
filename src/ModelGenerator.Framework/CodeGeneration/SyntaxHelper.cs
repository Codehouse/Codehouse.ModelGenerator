using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ModelGenerator.Framework.CodeGeneration
{
    public static class SyntaxHelper
    {
        private const string SitecoreIndexFieldAttribute = "Sitecore.ContentSearch.IndexField";

        public static AccessorDeclarationSyntax AutoGet()
        {
            return AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
               .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        public static FieldDeclarationSyntax FieldDeclaration(TypeSyntax typeName, string name)
        {
            return SyntaxFactory.FieldDeclaration(VariableDeclaration(typeName, name));
        }

        public static LiteralExpressionSyntax IdLiteral(Guid value)
        {
            return StringLiteral(value.ToSitecoreId());
        }

        public static SyntaxTrivia NewLineTrivia()
        {
            return EndOfLine(Environment.NewLine);
        }

        public static SyntaxTrivia[] NewLineTrivia(int count)
        {
            return Enumerable.Range(0, count)
                             .Select(i => NewLineTrivia())
                             .ToArray();
        }

        public static ParameterSyntax Parameter(string name, string typeName)
        {
            return SyntaxFactory.Parameter(Identifier(name))
                                .WithType(ParseTypeName(typeName));
        }

        public static AttributeSyntax SitecoreIndexField(string fieldName)
        {
            return Attribute(ParseName(SitecoreIndexFieldAttribute))
               .AddArgumentListArguments(
                    AttributeArgument(StringLiteral(fieldName.ToLowerInvariant()))
                );
        }

        public static LiteralExpressionSyntax StringLiteral(string value)
        {
            return LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));
        }

        public static VariableDeclarationSyntax VariableDeclaration(TypeSyntax typeName, string name)
        {
            return SyntaxFactory.VariableDeclaration(typeName)
                                .AddVariables(VariableDeclarator(Identifier(name)));
        }

        public static SyntaxTrivia Whitespace(int length = 1)
        {
            return SyntaxFactory.Whitespace(new string(' ', length));
        }
    }
}