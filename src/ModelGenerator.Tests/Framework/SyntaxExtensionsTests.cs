using System.Drawing.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Framework.CodeGeneration;
using NUnit.Framework;
using Shouldly;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static ModelGenerator.Framework.CodeGeneration.SyntaxHelper;

namespace ModelGenerator.Tests.Framework
{
    [TestFixture]
    public class SyntaxExtensionsTests
    {
        [Test]
        public void AttributeAddSimpleArguments_GivenNoArguments_AddsNoArguments()
        {
            var attribute = Attribute(ParseName("Foo"));
            var result = attribute.AddSimpleArguments();

            result.ArgumentList.ShouldNotBeNull();
            result.ArgumentList.Arguments.ShouldBeEmpty();
        }
        
        [Test]
        public void AttributeAddSimpleArguments_GivenArgument_AddsArgument()
        {
            var attribute = Attribute(ParseName("Foo"));
            var argument = StringLiteral("foo");
            var result = attribute.AddSimpleArguments(argument);

            result.ArgumentList.ShouldNotBeNull();
            result.ArgumentList.Arguments
                  .ShouldHaveSingleItem()
                  .Expression
                  .ShouldBeAssignableTo<LiteralExpressionSyntax>()!
                  .Token
                  .ValueText
                  .ShouldBe("foo");
        }

        [Test]
        public void AccessorDeclarationAddSingleAttribute_GivenAttribute_AddsAttribute()
        {
            const string attributeName = "FooAttribute";
            var attribute = Attribute(ParseName(attributeName));
            var accessor = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration);
            
            var result = accessor.AddSingleAttributes(attribute);
            
            result.ShouldBeOfType<AccessorDeclarationSyntax>("Node returned should be the same type as the one passed")
                  .AttributeLists
                  .ShouldHaveSingleItem()
                  .Attributes
                  .ShouldHaveSingleItem()
                  .Name.ToString().ShouldBe(attributeName);
        }

        [Test]
        public void AccessorDeclarationAddSingleAttribute_GivenMultipleAttributes_AddsMultipleAttributeListsWithSingleAttribute()
        {
            const string attributeName = "FooAttribute";
            const int attributeCount = 5;
            var attributes = Enumerable.Repeat(Attribute(ParseName(attributeName)), attributeCount).ToArray();
            var accessor = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration);
            
            var result = accessor.AddSingleAttributes(attributes);
            
            result.AttributeLists.Count.ShouldBe(attributeCount);
            result.AttributeLists.ShouldAllBe(l => l.Attributes.Count == 1, "Each attribute list should have only one attribute.");
        }

        [Test]
        public void ClassDeclarationAddSingleAttribute_GivenAttribute_AddsAttribute()
        {
            const string attributeName = "FooAttribute";
            var attribute = Attribute(ParseName(attributeName));
            var classDeclaration = ClassDeclaration("Foo");
            
            var result = classDeclaration.AddSingleAttributes(attribute);
            
            result.ShouldBeOfType<ClassDeclarationSyntax>("Node returned should be the same type as the one passed")
                  .AttributeLists
                  .ShouldHaveSingleItem()
                  .Attributes
                  .ShouldHaveSingleItem()
                  .Name.ToString().ShouldBe(attributeName);
        }

        [Test]
        public void InterfaceDeclarationAddSingleAttribute_GivenAttribute_AddsAttribute()
        {
            const string attributeName = "FooAttribute";
            var attribute = Attribute(ParseName(attributeName));
            var interfaceDeclaration = InterfaceDeclaration("Foo");
            
            var result = interfaceDeclaration.AddSingleAttributes(attribute);
            
            result.ShouldBeOfType<InterfaceDeclarationSyntax>("Node returned should be the same type as the one passed")
                  .AttributeLists
                  .ShouldHaveSingleItem()
                  .Attributes
                  .ShouldHaveSingleItem()
                  .Name.ToString().ShouldBe(attributeName);
        }

        [Test]
        public void PropertyDeclarationAddSingleAttribute_GivenAttribute_AddsAttribute()
        {
            const string attributeName = "FooAttribute";
            var attribute = Attribute(ParseName(attributeName));
            var propertyDeclaration = PropertyDeclaration(ParseTypeName("Foo"), "Foo");
            
            var result = propertyDeclaration.AddSingleAttributes(attribute);
            
            result.ShouldBeOfType<PropertyDeclarationSyntax>("Node returned should be the same type as the one passed")
                  .AttributeLists
                  .ShouldHaveSingleItem()
                  .Attributes
                  .ShouldHaveSingleItem()
                  .Name.ToString().ShouldBe(attributeName);
        }
        
        [Test]
        public void ConstructorInitializerAddArgumentList_GivenNoArguments_AddsNoArguments()
        {
            var ctor = ConstructorInitializer(SyntaxKind.BaseConstructorInitializer);
            var result = ctor.AddArgumentList();

            result.ArgumentList.ShouldNotBeNull();
            result.ArgumentList.Arguments.ShouldBeEmpty();
        }
        
        [Test]
        public void ConstructorInitializerAddArgumentList_GivenSingleStringArgument_AddsNamedArgument()
        {
            var ctor = ConstructorInitializer(SyntaxKind.BaseConstructorInitializer);
            var result = ctor.AddArgumentList("foo");

            result.ArgumentList.ShouldNotBeNull();
            result.ArgumentList.Arguments
                  .ShouldHaveSingleItem()
                  .Expression
                  .ShouldBeAssignableTo<IdentifierNameSyntax>("Expected name identifier, did not get one")!
                  .Identifier
                  .ValueText
                  .ShouldBe("foo");
        }
        
        [Test]
        public void ConstructorInitializerAddArgumentList_GivenSingleNullArgument_AddsNullLiteralArgument()
        {
            var ctor = ConstructorInitializer(SyntaxKind.BaseConstructorInitializer);
            var result = ctor.AddArgumentList(new string?[]{null});

            result.ArgumentList.ShouldNotBeNull();
            result.ArgumentList.Arguments
                  .ShouldHaveSingleItem()
                  .Expression
                  .ShouldBeAssignableTo<LiteralExpressionSyntax>("Expected literal expression, did not get one")!
                  .Token
                  .ValueText
                  .ShouldBe("null");
        }
    }
}