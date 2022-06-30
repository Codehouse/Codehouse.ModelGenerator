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
    public class FortisClassGenerator : FortisTypeGeneratorBase
    {
        public override string Tag => "Fortis.Class";

        private readonly IFortisFieldNameResolver _fieldNameResolver;
        private readonly FieldTypeResolver _fieldTypeResolver;
        private readonly FortisSettings _settings;
        private readonly TypeNameResolver _typeNameResolver;
        private readonly IXmlDocumentationGenerator _xmlDocGenerator;
        private const string SitecorePredefinedQueryAttribute = "PredefinedQuery";

        public FortisClassGenerator(IFortisFieldNameResolver fieldNameResolver, FieldTypeResolver fieldTypeResolver, FortisSettings settings, TypeNameResolver typeNameResolver, IXmlDocumentationGenerator xmlDocGenerator)
            : base(settings, typeNameResolver)
        {
            _fieldNameResolver = fieldNameResolver;
            _fieldTypeResolver = fieldTypeResolver;
            _settings = settings;
            _typeNameResolver = typeNameResolver;
            _xmlDocGenerator = xmlDocGenerator;
        }

        protected override (string? Name, TypeDeclarationSyntax? Class) GenerateTypeDeclaration(ScopedRagBuilder<string> statusTracker, GenerationContext context, ModelFile model)
        {
            var modelType = model.Types.Single();
            var typeName = _typeNameResolver.GetClassName(modelType.Template);
            var typeDeclaration =  ClassDeclaration(typeName)
                .AddModifiers(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.PartialKeyword))
                .AddBaseListTypes(GenerateBaseTypes(context, modelType.Template))
                .AddSingleAttributes(
                    CreateTemplateMappingAttribute(context, modelType.Template),
                    Attribute(ParseName(SitecorePredefinedQueryAttribute))
                        .AddArgumentListArguments(
                            AttributeArgument(StringLiteral("TemplateId")),
                            AttributeArgument(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("ComparisonType"),
                                    IdentifierName("Equal"))),
                            AttributeArgument(IdLiteral(modelType.Template.Id)),
                            AttributeArgument(TypeOfExpression(ParseTypeName("Guid")))
                        )
                )
                .AddMembers(context.Templates.GetAllFields(modelType.Template.Id)
                    .SelectMany(f => GenerateProperty(context, modelType, f))
                    .ToArray())
                .AddMembers(GenerateClassFields(context, modelType).ToArray())
                .AddMembers(GenerateConstructors(context, modelType).ToArray())
                .WithLeadingTrivia(_xmlDocGenerator.GetTemplateComment(modelType.Template));

            return (typeName, typeDeclaration);
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
                   .Where(t => !t.IsWellKnown)
                   .Select(t => GetBaseTypeName(template, t, context.Templates))
                   .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                   .Prepend(_typeNameResolver.GetInterfaceName(template))
                   .Prepend(isRenderingParameters ? _settings.TypeNames.RenderingParameter : _settings.TypeNames.ItemWrapper)
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

            yield return FieldDeclaration(ParseTypeName(_settings.TypeNames.SitecoreItem), "_item")
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
                                 Parameter("spawnProvider", _settings.TypeNames.SpawnProvider)
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
                                 Parameter("spawnProvider", _settings.TypeNames.SpawnProvider)
                             )
                             .WithBaseInitializer(null, "spawnProvider")
                             .WithBody(Block());

                // Ctor with id, ISpawnProvider
                yield return ConstructorDeclaration(Identifier(className))
                             .AddModifiers(Token(SyntaxKind.PublicKeyword))
                             .AddParameterListParameters(
                                 Parameter("id", "Guid"),
                                 Parameter("spawnProvider", _settings.TypeNames.SpawnProvider)
                             )
                             .WithBaseInitializer("id", "spawnProvider")
                             .WithBody(Block());

                // Ctor with id, field dictionary, ISpawnProvider
                yield return ConstructorDeclaration(Identifier(className))
                             .AddModifiers(Token(SyntaxKind.PublicKeyword))
                             .AddParameterListParameters(
                                 Parameter("id", "Guid"),
                                 Parameter("lazyFields", "Dictionary<string, object>"),
                                 Parameter("spawnProvider", _settings.TypeNames.SpawnProvider)
                             )
                             .WithBaseInitializer("id", "lazyFields", "spawnProvider")
                             .WithBody(Block());

                // Ctor with item, ISpawnProvider
                yield return ConstructorDeclaration(Identifier(className))
                             .AddModifiers(Token(SyntaxKind.PublicKeyword))
                             .AddParameterListParameters(
                                 Parameter("item", "Item"),
                                 Parameter("spawnProvider", _settings.TypeNames.SpawnProvider)
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

            yield return GeneratePropertyField(model.Template, templateField, isRenderingParameters, concreteType, fieldComment);

            var valueType = _fieldTypeResolver.GetFieldValueType(templateField);
            if (valueType != null)
            {
                yield return GeneratePropertyFieldValue(model.Template, templateField, valueType, isRenderingParameters, fieldComment);
            }
        }

        private PropertyDeclarationSyntax GeneratePropertyField(Template template, TemplateField templateField, bool isRenderingParameters, string concreteType, SyntaxTriviaList fieldComment)
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

        private PropertyDeclarationSyntax GeneratePropertyFieldValue(Template template, TemplateField templateField, string valueType, bool isRenderingParameters, SyntaxTriviaList fieldComment)
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

        private string GetBaseTypeName(Template currentTemplate, Template baseTemplate, TemplateCollection collection)
        {
            var baseTemplateSet = collection.TemplateSets[baseTemplate.SetId];
            return _typeNameResolver.GetRelativeInterfaceName(currentTemplate, baseTemplate, baseTemplateSet);
        }
    }
}