using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ModelGenerator.Framework.CodeGeneration
{
    public static class SyntaxExtensions
    {
        public static ConstructorInitializerSyntax AddArgumentList(this ConstructorInitializerSyntax syntax, params string?[] names)
        {
            var arguments = names
                           .Select(n => n is null
                                ? SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                                : (ExpressionSyntax)SyntaxFactory.IdentifierName(n))
                           .Select(SyntaxFactory.Argument)
                           .ToArray();
            return syntax.AddArgumentListArguments(arguments);
        }

        public static AttributeSyntax AddSimpleArguments(this AttributeSyntax member, params ExpressionSyntax[] expressionArguments)
        {
            var arguments = expressionArguments
                           .Select(SyntaxFactory.AttributeArgument)
                           .ToArray();
            return member.AddArgumentListArguments(arguments);
        }

        public static T AddSingleAttributes<T>(this T syntax, params AttributeSyntax[] attributes)
            where T : SyntaxNode
        {
            var attributeLists = attributes
                                .Select(a => SyntaxFactory.AttributeList().AddAttributes(a))
                                .ToArray();
            SyntaxNode node = syntax switch
            {
                AccessorDeclarationSyntax accessor => accessor.AddAttributeLists(attributeLists),
                ClassDeclarationSyntax @class => @class.AddAttributeLists(attributeLists),
                InterfaceDeclarationSyntax @interface => @interface.AddAttributeLists(attributeLists),
                PropertyDeclarationSyntax property => property.AddAttributeLists(attributeLists),
                _ => throw new NotSupportedException($"Unsupported syntax type: {syntax.GetType().Name}.")
            };

            return (T)node;
        }

        public static T If<T>(this T syntax, bool condition, Func<T, T> mutation)
            where T : SyntaxNode
        {
            return condition
                ? mutation.Invoke(syntax)
                : syntax;
        }

        public static ConstructorDeclarationSyntax WithBaseInitializer(this ConstructorDeclarationSyntax syntax, params string?[] names)
        {
            return syntax.WithInitializer(
                SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                             .AddArgumentList(names)
            );
        }
    }
}