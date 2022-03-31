using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Fortis.Configuration;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.Progress;
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

        public IEnumerable<MemberDeclarationSyntax> GenerateCode(ScopedRagBuilder<string> ragBuilder, GenerationContext context, ModelType model)
        {
            return GenerateCode(ragBuilder, context, new[] { model });
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateCode(ScopedRagBuilder<string> ragBuilder, GenerationContext context, IEnumerable<ModelType> models)
        {
            yield return GenerateTemplateIdClass(context, models);

            if (models.SelectMany(m => m.Template.OwnFields).Any())
            {
                yield return GenerateFieldIdClasses(ragBuilder, context, models);
            }
        }

        private ClassDeclarationSyntax GenerateFieldIdClasses(ScopedRagBuilder<string> ragBuilder, GenerationContext context, IEnumerable<ModelType> models)
        {
            var innerClasses = models
                               .Select(m => GenerateFieldIdInnerClass(ragBuilder, context, m))
                               .ToArray();

            return ClassDeclaration(Identifier("FieldIds"))
                   .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.PartialKeyword))
                   .AddMembers(innerClasses);
        }

        private ClassDeclarationSyntax GenerateFieldIdInnerClass(ScopedRagBuilder<string> ragBuilder, GenerationContext context, ModelType model)
        {
            var typeName = _typeNameResolver.GetTypeName(model.Template);
            var fieldProperties = model.Template.OwnFields
                                       .Select(p => GenerateIdProperty(ragBuilder, model.Template, p, typeName))
                                       .ToArray();

            return ClassDeclaration(Identifier(typeName))
                   .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                   .AddMembers(fieldProperties)
                   .WithLeadingTrivia(_xmlDocGenerator.GetTemplateComment(model.Template));
        }

        private PropertyDeclarationSyntax GenerateIdProperty(ScopedRagBuilder<string> ragBuilder, Template template, TemplateField field, string typeName)
        {
            var propertyName = _fieldNameResolver.GetFieldName(field);
            if (propertyName.Equals(typeName))
            {
                ragBuilder.AddWarn($"Template contains field with same name as generated class ({typeName}).");
                propertyName += "FieldId";
            }
            
            return GenerateIdProperty(propertyName, field.Id)
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