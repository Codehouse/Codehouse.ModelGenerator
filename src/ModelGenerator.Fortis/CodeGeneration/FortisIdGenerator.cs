using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Fortis.Configuration;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.TypeConstruction;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static ModelGenerator.Framework.CodeGeneration.SyntaxHelper;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FortisIdGenerator : FortisTypeGeneratorBase
    {
        private readonly FieldNameResolver _fieldNameResolver;
        private readonly TypeNameResolver _typeNameResolver;
        private readonly IXmlDocumentationGenerator _xmlDocGenerator;

        public FortisIdGenerator(FieldNameResolver fieldNameResolver, FortisSettings settings, TypeNameResolver typeNameResolver, IXmlDocumentationGenerator xmlDocGenerator)
            : base(settings)
        {
            _fieldNameResolver = fieldNameResolver;
            _typeNameResolver = typeNameResolver;
            _xmlDocGenerator = xmlDocGenerator;
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateCode(GenerationContext context, ModelType model)
        {
            return GenerateCode(context, new[] { model });
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateCode(GenerationContext context, IEnumerable<ModelType> models)
        {
            yield return GenerateTemplateIdClass(context, models);

            if (models.SelectMany(m => m.Template.OwnFields).Any())
            {
                yield return GenerateFieldIdClasses(context, models);
            }
        }

        private ClassDeclarationSyntax GenerateFieldIdClasses(GenerationContext context, IEnumerable<ModelType> models)
        {
            var innerClasses = models
                               .Select(m => GenerateFieldIdInnerClass(context, m))
                               .ToArray();

            return ClassDeclaration(Identifier("FieldIds"))
                   .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.PartialKeyword))
                   .AddMembers(innerClasses);
        }

        private ClassDeclarationSyntax GenerateFieldIdInnerClass(GenerationContext context, ModelType model)
        {
            var fieldProperties = model.Template.OwnFields
                                       .Select(p => GenerateIdProperty(model.Template, p))
                                       .ToArray();

            return ClassDeclaration(Identifier(_typeNameResolver.GetTypeName(model.Template)))
                   .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                   .AddMembers(fieldProperties)
                   .WithLeadingTrivia(_xmlDocGenerator.GetTemplateComment(model.Template));
        }

        private PropertyDeclarationSyntax GenerateIdProperty(Template template, TemplateField field)
        {
            return GenerateIdProperty(_fieldNameResolver.GetFieldName(field), field.Id)
                .WithLeadingTrivia(_xmlDocGenerator.GetFieldComment(template, field));
        }

        private PropertyDeclarationSyntax GenerateIdProperty(Template template)
        {
            return GenerateIdProperty(_typeNameResolver.GetTypeName(template), template.Id)
                .WithLeadingTrivia(_xmlDocGenerator.GetTemplateComment(template));
        }

        private PropertyDeclarationSyntax GenerateIdProperty(string name, Guid value)
        {
            return PropertyDeclaration(IdentifierName("Guid"), name)
                   .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                   .AddAccessorListAccessors(AutoGet())
                   .WithInitializer(
                       EqualsValueClause(
                           InvocationExpression(
                                   MemberAccessExpression(
                                       SyntaxKind.SimpleMemberAccessExpression,
                                       IdentifierName("Guid"),
                                       IdentifierName("Parse")
                                   )
                               )
                               .AddArgumentListArguments(Argument(IdLiteral(value)))
                       )
                   )
                   .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        private ClassDeclarationSyntax GenerateTemplateIdClass(GenerationContext context, IEnumerable<ModelType> models)
        {
            var properties = models
                             .Select(model => GenerateIdProperty(model.Template))
                             .ToArray();

            return ClassDeclaration(Identifier("TemplateIds"))
                   .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.PartialKeyword))
                   .AddMembers(properties);
        }
    }
}