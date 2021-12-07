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
    public class FortisClassGenerator : FortisTypeGeneratorBase
    {
        private readonly FieldNameResolver _fieldNameResolver;
        private readonly FieldTypeResolver _fieldTypeResolver;
        private readonly FortisSettings _settings;
        private readonly TypeNameResolver _typeNameResolver;
        private readonly IXmlDocumentationGenerator _xmlDocGenerator;
        private const string SitecorePredefinedQueryAttribute = "PredefinedQuery";

        public FortisClassGenerator(FieldNameResolver fieldNameResolver, FieldTypeResolver fieldTypeResolver, FortisSettings settings, TypeNameResolver typeNameResolver, IXmlDocumentationGenerator xmlDocGenerator)
        {
            _fieldNameResolver = fieldNameResolver;
            _fieldTypeResolver = fieldTypeResolver;
            _settings = settings;
            _typeNameResolver = typeNameResolver;
            _xmlDocGenerator = xmlDocGenerator;
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateCode(GenerationContext context, ModelType model)
        {
            var type = ClassDeclaration(_typeNameResolver.GetClassName(model.Template))
                       .AddModifiers(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.PartialKeyword))
                       .AddBaseListTypes(GenerateBaseTypes(context, model.Template))
                       .AddSingleAttributes(
                           CreateTemplateMappingAttribute(context, model.Template),
                           Attribute(ParseName(SitecorePredefinedQueryAttribute))
                               .AddArgumentListArguments(
                                   AttributeArgument(StringLiteral("TemplateId")),
                                   AttributeArgument(
                                       MemberAccessExpression(
                                           SyntaxKind.SimpleMemberAccessExpression,
                                           IdentifierName("ComparisonType"),
                                           IdentifierName("Equal"))),
                                   AttributeArgument(IdLiteral(model.Template.Id)),
                                   AttributeArgument(TypeOfExpression(ParseTypeName("Guid")))
                               )
                       )
                       .AddMembers(GenerateClassFields(context, model).ToArray())
                       .AddMembers(GenerateConstructors(context, model).ToArray())
                       .AddMembers(context.Templates.GetAllFields(model.Template.Id)
                                          .SelectMany(f => GenerateProperty(context, model, f))
                                          .ToArray())
                       .WithLeadingTrivia(_xmlDocGenerator.GetTemplateComment(model.Template));

            yield return type;
        }

        private AttributeSyntax CreateTemplateMappingAttribute(GenerationContext context, Template template)
        {
            var mappingType = context.Templates.GetTemplateType(template.Id) == TemplateTypes.RenderingParameter
                ? "RenderingParameter"
                : string.Empty;

            return CreateTemplateMappingAttribute(template.Id, mappingType);
        }

        private SimpleBaseTypeSyntax[] GenerateBaseTypes(GenerationContext context, Template template)
        {
            var isRenderingParameters = context.Templates.IsRenderingParameters(template.Id);
            var baseTemplates = _settings.Quirks.FullInterfaceList
                ? context.Templates.GetAllBaseTemplates(template.Id)
                : template.BaseTemplateIds
                          .Where(tid => context.Templates.Templates.ContainsKey(tid))
                          .Select(tid => context.Templates.Templates[tid]);

            return baseTemplates
                   .Where(t => t.SetId != null)
                   .Select(t => GetBaseTypeName(template, t, context.Templates))
                   .Prepend(_typeNameResolver.GetInterfaceName(template))
                   .Prepend(isRenderingParameters ? "RenderingParameter" : "FortisItem")
                   .Select(typeName => SimpleBaseType(ParseTypeName(typeName)))
                   .ToArray();
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateClassFields(GenerationContext context, ModelType model)
        {
            var isRenderingParameters = context.Templates.IsRenderingParameters(model.Template.Id);
            if (isRenderingParameters)
            {
                yield break;
            }

            yield return FieldDeclaration(ParseTypeName("Item"), "_item")
                         .AddModifiers(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword))
                         .WithTrailingTrivia(EndOfLine(string.Empty));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateConstructors(GenerationContext context, ModelType model)
        {
            var className = _typeNameResolver.GetClassName(model.Template);
            var isRenderingParameters = context.Templates.IsRenderingParameters(model.Template.Id);

            if (isRenderingParameters)
            {
                yield return ConstructorDeclaration(Identifier(className))
                             .AddModifiers(Token(SyntaxKind.PublicKeyword))
                             .AddParameterListParameters(
                                 Parameter("parameters", "Dictionary<string, string>"),
                                 Parameter("spawnProvider", "ISpawnProvider")
                             )
                             .WithBaseInitializer("parameters", "spawnProvider")
                             .WithBody(Block());
            }
            else
            {
                // Ctor with ISpawnProvider
                yield return ConstructorDeclaration(Identifier(className))
                             .AddModifiers(Token(SyntaxKind.PublicKeyword))
                             .AddParameterListParameters(
                                 Parameter("spawnProvider", "ISpawnProvider")
                             )
                             .WithBaseInitializer(null, "spawnProvider")
                             .WithBody(Block());

                // Ctor with id, ISpawnProvider
                yield return ConstructorDeclaration(Identifier(className))
                             .AddModifiers(Token(SyntaxKind.PublicKeyword))
                             .AddParameterListParameters(
                                 Parameter("id", "Guid"),
                                 Parameter("spawnProvider", "ISpawnProvider")
                             )
                             .WithBaseInitializer("id", "spawnProvider")
                             .WithBody(Block());

                // Ctor with id, field dictionary, ISpawnProvider
                yield return ConstructorDeclaration(Identifier(className))
                             .AddModifiers(Token(SyntaxKind.PublicKeyword))
                             .AddParameterListParameters(
                                 Parameter("id", "Guid"),
                                 Parameter("lazyFields", "Dictionary<string, object>"),
                                 Parameter("spawnProvider", "ISpawnProvider")
                             )
                             .WithBaseInitializer("id", "lazyFields", "spawnProvider")
                             .WithBody(Block());

                // Ctor with item, ISpawnProvider
                yield return ConstructorDeclaration(Identifier(className))
                             .AddModifiers(Token(SyntaxKind.PublicKeyword))
                             .AddParameterListParameters(
                                 Parameter("item", "Item"),
                                 Parameter("spawnProvider", "ISpawnProvider")
                             )
                             .WithBaseInitializer("item", "spawnProvider")
                             .WithBody(
                                 Block()
                                     .AddStatements(
                                         ExpressionStatement(
                                             AssignmentExpression(
                                                 SyntaxKind.SimpleAssignmentExpression,
                                                 IdentifierName("_item"),
                                                 IdentifierName("item")
                                             )
                                         )
                                     )
                             );
            }
        }

        private MemberDeclarationSyntax[] GenerateMembers(GenerationContext context, ModelType model, IEnumerable<TemplateField> fields)
        {
            return GenerateClassFields(context, model)
                   .Union(GenerateConstructors(context, model))
                   .Union(fields.SelectMany(f => GenerateProperty(context, model, f)))
                   .ToArray();
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateProperty(GenerationContext context, ModelType model, TemplateField templateField)
        {
            var isRenderingParameters = context.Templates.IsRenderingParameters(model.Template.Id);
            var concreteType = isRenderingParameters
                ? _fieldTypeResolver.GetFieldParameterType(templateField)
                : _fieldTypeResolver.GetFieldConcreteType(templateField);
            var template = model.Template.Id == templateField.TemplateId
                ? model.Template
                : context.Templates.Templates[templateField.TemplateId];
            var fieldComment = _xmlDocGenerator.GetFieldComment(template, templateField);

            // TODO: Fix spacing on property accessors.
            yield return GeneratePropertyField(model.Template, templateField, isRenderingParameters, concreteType, fieldComment);

            var valueType = _fieldTypeResolver.GetFieldValueType(templateField);
            if (valueType != null)
            {
                yield return GeneratePropertyFieldValue(model.Template, templateField, valueType, isRenderingParameters, fieldComment);
            }
        }

        private PropertyDeclarationSyntax GeneratePropertyFieldValue(Template template, TemplateField templateField, string? valueType, bool isRenderingParameters, SyntaxTriviaList fieldComment)
        {
            try
            {
                return PropertyDeclaration(ParseTypeName(valueType), _fieldNameResolver.GetFieldValueName(templateField))
                       .AddModifiers(Token(SyntaxKind.PublicKeyword))
                       .AddAccessorListAccessors(
                           AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                               .AddSingleAttributes(Attribute(ParseName("DebuggerStepThrough")))
                               .WithExpressionBody(
                                   ArrowExpressionClause(
                                       MemberAccessExpression(
                                           SyntaxKind.SimpleMemberAccessExpression,
                                           IdentifierName(_fieldNameResolver.GetFieldName(templateField)),
                                           IdentifierName("Value"))
                                   )
                               )
                               .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                       )
                       .If(
                           !isRenderingParameters,
                           property => property.AddSingleAttributes(SitecoreIndexField(templateField.Name)))
                       .WithLeadingTrivia(fieldComment);
            }
            catch (Exception ex)
            {
                throw new GenerationException($"Could not generate field value property for field {templateField.Name} on template {template.Name}.", ex);
            }
        }

        private PropertyDeclarationSyntax GeneratePropertyField(Template template, TemplateField templateField, bool isRenderingParameters, string? concreteType, SyntaxTriviaList fieldComment)
        {
            try
            {
                return PropertyDeclaration(ParseTypeName(_fieldTypeResolver.GetFieldInterfaceType(templateField)), _fieldNameResolver.GetFieldName(templateField))
                       .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.VirtualKeyword))
                       .AddAccessorListAccessors(
                           AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                               .AddSingleAttributes(Attribute(ParseName("DebuggerStepThrough")))
                               .WithExpressionBody(
                                   ArrowExpressionClause(GetFieldInvocation(isRenderingParameters, templateField, concreteType))
                               )
                               .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                       )
                       .If(
                           !isRenderingParameters,
                           property => property.AddSingleAttributes(SitecoreIndexField(templateField.Name)))
                       .WithLeadingTrivia(fieldComment);
            }
            catch (Exception ex)
            {
                throw new GenerationException($"Could not generate field property for field {templateField.Name} on template {template.Name}.", ex);
            }
        }

        private string GetBaseTypeName(Template currentTemplate, Template baseTemplate, TemplateCollection collection)
        {
            var baseTemplateSet = collection.TemplateSets[baseTemplate.SetId];
            return _typeNameResolver.GetRelativeInterfaceName(currentTemplate, baseTemplate, baseTemplateSet);
        }
    }
}