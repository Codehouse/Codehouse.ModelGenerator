using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.TypeConstruction;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static ModelGenerator.Framework.CodeGeneration.SyntaxHelper;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FortisIdGenerator : IGenerator<ModelIdType, MemberDeclarationSyntax>
    {
        private readonly FieldNameResolver _fieldNameResolver;
        private readonly TypeNameResolver _typeNameResolver;
        private readonly XmlDocGenerator _xmlDocGenerator;

        public FortisIdGenerator(FieldNameResolver fieldNameResolver, TypeNameResolver typeNameResolver, XmlDocGenerator xmlDocGenerator)
        {
            _fieldNameResolver = fieldNameResolver;
            _typeNameResolver = typeNameResolver;
            _xmlDocGenerator = xmlDocGenerator;
        }
        
        public IEnumerable<MemberDeclarationSyntax> GenerateCode(GenerationContext context, ModelIdType model)
        {
            yield return GenerateTemplateIdClass(context, model);
            yield return GenerateFieldIdClass(context, model);
        }

        private ClassDeclarationSyntax GenerateFieldIdClass(GenerationContext context, ModelIdType model)
        {
            var fieldProperties = model.Template.OwnFields
                                       .Select(GenerateIdProperty)
                                       .ToArray();
            
            return ClassDeclaration(Identifier("FieldIds"))
                   .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.PartialKeyword))
                   .AddMembers(
                       ClassDeclaration(Identifier(_typeNameResolver.GetTypeName(model.Template)))
                           .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                           .AddMembers(fieldProperties)
                           .WithLeadingTrivia(_xmlDocGenerator.GenerateTemplateIdComment(model.Template))
                   );
        }

        private ClassDeclarationSyntax GenerateTemplateIdClass(GenerationContext context, ModelIdType model)
        {
            return ClassDeclaration(Identifier("TemplateIds"))
                   .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.PartialKeyword))
                   .AddMembers(GenerateIdProperty(model.Template));
        }

        private PropertyDeclarationSyntax GenerateIdProperty(TemplateField field)
        {
            return GenerateIdProperty(_fieldNameResolver.GetFieldName(field), field.Id)
                .WithLeadingTrivia(_xmlDocGenerator.GenerateFieldIdComment(field));
        }
        private PropertyDeclarationSyntax GenerateIdProperty(Template template)
        {
            return GenerateIdProperty(_typeNameResolver.GetTypeName(template), template.Id)
                .WithLeadingTrivia(_xmlDocGenerator.GenerateTemplateIdComment(template));
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
    }
}