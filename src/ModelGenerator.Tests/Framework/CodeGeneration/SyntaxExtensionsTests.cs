using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Framework.CodeGeneration;
using NUnit.Framework;
using Shouldly;

namespace ModelGenerator.Tests.Framework.CodeGeneration
{
    [TestFixture]
    public class SyntaxExtensionsTests
    {
        [Test]
        public void AddArgumentList_GivenNames_AddsAppropriateArguments()
        {
            var constructor = SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer);
            var names = new[] { "foo", "bar", "bas" };

            var result = constructor.AddArgumentList(names);

            // Extract the names of the arguments from the output syntax tree
            result.ArgumentList.Arguments
                  .Select(n => n.Expression)
                  .Cast<IdentifierNameSyntax>()
                  .Select(n => n.Identifier.Text)
                  .ToArray()
                  .ShouldBeEquivalentTo(names);
            result.ArgumentList.Arguments.Count.ShouldBe(names.Length);
        }

        [Test]
        public void AddArgumentList_GivenNoArguments_AddsNoArguments()
        {
            var constructor = SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer);

            var result = constructor.AddArgumentList();

            result.ArgumentList.Arguments.ShouldBeEmpty();
        }

        [Test]
        public void AddArgumentList_GivenNull_AddsNullLiteralArgument()
        {
            var constructor = SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer);
            var names = new string?[] { null, null };

            var result = constructor.AddArgumentList(names);

            result.ArgumentList.Arguments.Count.ShouldBe(names.Length);
            result.ArgumentList.Arguments.ShouldAllBe(n => n.Expression.Kind() == SyntaxKind.NullLiteralExpression);
        }

        [Test]
        public void AddSimpleArguments_GivenArgument_AddsArgument()
        {
            ExpressionSyntax argument = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("foo"));
            var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Foo"));

            var result = attribute.AddSimpleArguments(argument);

            result.ArgumentList!.Arguments.Single().Expression.ToString().ShouldBeEquivalentTo(argument.ToString());
        }

        [Test]
        public void AddSimpleArguments_GivenNoArguments_AddsNoArguments()
        {
            var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Foo"));

            var result = attribute.AddSimpleArguments();

            result.ArgumentList!.Arguments.ShouldBeEmpty();
        }
    }
}