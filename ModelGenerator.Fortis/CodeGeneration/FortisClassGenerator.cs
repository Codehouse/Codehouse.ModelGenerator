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
    public class FortisClassGenerator : IGenerator<ModelClass, MemberDeclarationSyntax>
    {
        private const string FortisModelTemplateMappingAttribute = "TemplateMapping";
        private const string SitecorePredefinedQueryAttribute = "PredefinedQuery";
        private readonly FieldNameResolver _fieldNameResolver;
        private readonly FieldTypeResolver _fieldTypeResolver;
        private readonly TypeNameResolver _typeNameResolver;
        private readonly XmlDocGenerator _xmlDocGenerator;

        public FortisClassGenerator(FieldNameResolver fieldNameResolver, FieldTypeResolver fieldTypeResolver, TypeNameResolver typeNameResolver, XmlDocGenerator xmlDocGenerator)
        {
            _fieldNameResolver = fieldNameResolver;
            _fieldTypeResolver = fieldTypeResolver;
            _typeNameResolver = typeNameResolver;
            _xmlDocGenerator = xmlDocGenerator;
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateCode(GenerationContext context, ModelClass model)
        {
            var type = ClassDeclaration(_typeNameResolver.GetClassName(model.Template))
                       .AddModifiers(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.PartialKeyword))
                       .AddBaseListTypes(GenerateBaseTypes(context, model.Template))
                       .AddSingleAttributes(
                           Attribute(ParseName(FortisModelTemplateMappingAttribute))
                               .AddArgumentListArguments(
                                   AttributeArgument(IdLiteral(model.Template.Id)),
                                   AttributeArgument(StringLiteral(string.Empty))
                               ),
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
                       .AddMembers(GenerateMembers(context, model, context.Templates.GetAllFields(model.Template.Id)))
                       .WithLeadingTrivia(_xmlDocGenerator.GenerateClassComment(model.Template));

            yield return type;
        }

        private MemberDeclarationSyntax[] GenerateMembers(GenerationContext context, ModelClass model, IEnumerable<TemplateField> fields)
        {
            return GenerateClassFields(context, model)
                   .Union(GenerateConstructors(context, model))
                   .Union(fields.SelectMany(f => GenerateProperty(context, model, f)))
                   .ToArray();
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateClassFields(GenerationContext context, ModelClass model)
        {
            var isRenderingParameters = context.Templates.IsRenderingParameters(model.Template.Id);
            if (isRenderingParameters)
            {
                yield break;
            }
            
            yield return FieldDeclaration(
                VariableDeclaration(ParseTypeName("Item"))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier("_item"))
                        )
                    )
                )
                .AddModifiers(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword))
                .WithTrailingTrivia(EndOfLine(string.Empty));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateConstructors(GenerationContext context, ModelClass model)
        {
            var className = _typeNameResolver.GetClassName(model.Template);
            var isRenderingParameters = context.Templates.IsRenderingParameters(model.Template.Id);

            if (isRenderingParameters)
            {
                yield return ConstructorDeclaration(Identifier(className))
                             .AddModifiers(Token(SyntaxKind.PublicKeyword))
                             .AddParameterListParameters(
                                 Parameter(Identifier("parameters"))
                                     .WithType(ParseTypeName("Dictionary<string, string>")),
                                 Parameter(Identifier("spawnProvider"))
                                     .WithType(ParseTypeName("ISpawnProvider"))
                             )
                             .WithInitializer(
                                 ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                                     .AddArgumentListArguments(
                                         Argument(IdentifierName("parameters")),
                                         Argument(IdentifierName("spawnProvider"))
                                     )
                             )
                             .WithBody(Block());
            }
            else
            {

                // Ctor with ISpawnProvider
                yield return ConstructorDeclaration(Identifier(className))
                             .AddModifiers(Token(SyntaxKind.PublicKeyword))
                             .AddParameterListParameters(
                                 Parameter(Identifier("spawnProvider"))
                                     .WithType(ParseTypeName("ISpawnProvider"))
                             )
                             .WithInitializer(
                                 ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                                     .AddArgumentListArguments(
                                         Argument(LiteralExpression(SyntaxKind.NullLiteralExpression)),
                                         Argument(IdentifierName("spawnProvider"))
                                     )
                             )
                             .WithBody(Block());

                // Ctor with id, ISpawnProvider
                yield return ConstructorDeclaration(Identifier(className))
                             .AddModifiers(Token(SyntaxKind.PublicKeyword))
                             .AddParameterListParameters(
                                 Parameter(Identifier("id"))
                                     .WithType(ParseTypeName("Guid")),
                                 Parameter(Identifier("spawnProvider"))
                                     .WithType(ParseTypeName("ISpawnProvider"))
                             )
                             .WithInitializer(
                                 ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                                     .AddArgumentListArguments(
                                         Argument(IdentifierName("id")),
                                         Argument(IdentifierName("spawnProvider"))
                                     )
                             )
                             .WithBody(Block());

                // Ctor with id, field dictionary, ISpawnProvider
                yield return ConstructorDeclaration(Identifier(className))
                             .AddModifiers(Token(SyntaxKind.PublicKeyword))
                             .AddParameterListParameters(
                                 Parameter(Identifier("id"))
                                     .WithType(ParseTypeName("Guid")),
                                 Parameter(Identifier("lazyFields"))
                                     .WithType(ParseTypeName("Dictionary<string, object>")),
                                 Parameter(Identifier("spawnProvider"))
                                     .WithType(ParseTypeName("ISpawnProvider"))
                             )
                             .WithInitializer(
                                 ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                                     .AddArgumentListArguments(
                                         Argument(IdentifierName("id")),
                                         Argument(IdentifierName("lazyFields")),
                                         Argument(IdentifierName("spawnProvider"))
                                     )
                             )
                             .WithBody(Block());

                // Ctor with item, ISpawnProvider
                yield return ConstructorDeclaration(Identifier(className))
                             .AddModifiers(Token(SyntaxKind.PublicKeyword))
                             .AddParameterListParameters(
                                 Parameter(Identifier("item"))
                                     .WithType(ParseTypeName("Item")),
                                 Parameter(Identifier("spawnProvider"))
                                     .WithType(ParseTypeName("ISpawnProvider"))
                             )
                             .WithInitializer(
                                 ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                                     .AddArgumentListArguments(
                                         Argument(IdentifierName("item")),
                                         Argument(IdentifierName("spawnProvider"))
                                     )
                             )
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

        private IEnumerable<MemberDeclarationSyntax> GenerateProperty(GenerationContext context, ModelClass model, TemplateField templateField)
        {
            var isRenderingParameters = context.Templates.IsRenderingParameters(model.Template.Id);
            var concreteType = isRenderingParameters
                ? _fieldTypeResolver.GetFieldParameterType(templateField)
                : _fieldTypeResolver.GetFieldConcreteType(templateField);

            // TODO: Fix spacing on property accessors.
            yield return PropertyDeclaration(ParseTypeName(_fieldTypeResolver.GetFieldInterfaceType(templateField)), _fieldNameResolver.GetFieldName(templateField))
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
                         .WithLeadingTrivia(_xmlDocGenerator.GenerateFieldComment(model.Template, templateField));

            var valueType = _fieldTypeResolver.GetFieldValueType(templateField);
            if (valueType != null)
            {
                yield return PropertyDeclaration(ParseTypeName(valueType), _fieldNameResolver.GetFieldValueName(templateField))
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
                             .WithLeadingTrivia(_xmlDocGenerator.GenerateFieldComment(model.Template, templateField));
            }
        }

        private static InvocationExpressionSyntax GetFieldInvocation(bool isRenderingParameter, TemplateField templateField, string? concreteType)
        {
            var arguments = Enumerable.Empty<ArgumentSyntax>()
                                      .Append(Argument(StringLiteral(templateField.Name)));
            if (!isRenderingParameter)
            {
                arguments = arguments.Append(Argument(StringLiteral(templateField.Name.ToLowerInvariant())));
            }
            
            // GetField<TField>("FieldName", "fieldname") for normal templates
            // GetField<TField>("FieldName") for rendering parameter templates
            return InvocationExpression(
                    GenericName(Identifier("GetField"))
                        .AddTypeArgumentListArguments(IdentifierName(concreteType))
                )
                .AddArgumentListArguments(arguments.ToArray());
        }

        private SimpleBaseTypeSyntax[] GenerateBaseTypes(GenerationContext context, Template template)
        {
            var isRenderingParameters = context.Templates.IsRenderingParameters(template.Id);
            var baseTemplates = context.Templates.GetAllBaseTemplates(template.Id);

            return baseTemplates
                   .Where(t => t.SetId != null)
                   .Select(t => GetBaseTypeName(template, t, context.Templates))
                   .Prepend(_typeNameResolver.GetInterfaceName(template))
                   .Prepend(isRenderingParameters ? "RenderingParameter" : "FortisItem")
                   .Select(typeName => SimpleBaseType(ParseTypeName(typeName)))
                   .ToArray();
        }

        private string GetBaseTypeName(Template currentTemplate, Template baseTemplate, TemplateCollection collection)
        {
            var baseTemplateSet = collection.TemplateSets[baseTemplate.SetId];
            return _typeNameResolver.GetRelativeInterfaceName(currentTemplate, baseTemplate, baseTemplateSet);
        }
    }
}