using System;
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
        
        public static LiteralExpressionSyntax IdLiteral(Guid value)
        {
            return StringLiteral(value.ToString("B").ToUpperInvariant());
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
            return LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));
        }
    }
}