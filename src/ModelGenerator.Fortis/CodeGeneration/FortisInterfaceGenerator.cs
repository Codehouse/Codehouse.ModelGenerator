using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public class FortisInterfaceGenerator : FortisTypeGeneratorBase
    {
        private readonly FieldNameResolver _fieldNameResolver;
        private readonly FieldTypeResolver _fieldTypeResolver;
        private readonly FortisSettings _settings;
        private readonly TypeNameResolver _typeNameResolver;
        private readonly IXmlDocumentationGenerator _xmlDocGenerator;

        public FortisInterfaceGenerator(FieldNameResolver fieldNameResolver, FieldTypeResolver fieldTypeResolver, TypeNameResolver typeNameResolver, FortisSettings settings, IXmlDocumentationGenerator xmlDocGenerator)
        {
            _fieldNameResolver = fieldNameResolver;
            _fieldTypeResolver = fieldTypeResolver;
            _typeNameResolver = typeNameResolver;
            _settings = settings;
            _xmlDocGenerator = xmlDocGenerator;
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateCode(GenerationContext context, ModelType model)
        {
            var type = InterfaceDeclaration(_typeNameResolver.GetInterfaceName(model.Template))
                       .AddModifiers(Token(SyntaxKind.PublicKeyword))
                       .If(_settings.Quirks.PartialInterfaces, i => i.AddModifiers(Token(SyntaxKind.PartialKeyword)))
                       .AddBaseListTypes(GenerateBaseTypes(context, model.Template))
                       .AddSingleAttributes(CreateTemplateMappingAttribute(context, model.Template))
                       .AddMembers(GenerateFields(model, model.Template.OwnFields))
                       .WithLeadingTrivia(_xmlDocGenerator.GetTemplateComment(model.Template));

            yield return type;
        }

        private AttributeSyntax CreateTemplateMappingAttribute(GenerationContext context, Template template)
        {
            var mappingType = context.Templates.GetTemplateType(template.Id) == TemplateTypes.RenderingParameter
                ? "InterfaceRenderingParameter"
                : "InterfaceMap";

            return CreateTemplateMappingAttribute(template.Id, mappingType);
        }

        private SimpleBaseTypeSyntax[] GenerateBaseTypes(GenerationContext context, Template template)
        {
            var templates = context.Templates.Templates;
            var isRenderingParameters = context.Templates.IsRenderingParameters(template.Id);

            return template.BaseTemplateIds
                           .Where(id => templates.ContainsKey(id))
                           .Where(id => !templates[id].IsWellKnown)
                           .Select(id => GetBaseTypeName(template, templates[id], context.Templates))
                           .Prepend(isRenderingParameters ? "IRenderingParameter" : "IItem")
                           .Select(typeName => SimpleBaseType(ParseTypeName(typeName)))
                           .ToArray();
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateField(ModelType model, TemplateField templateField)
        {
            yield return GenerateFieldProperty(model.Template, templateField);

            var valueType = _fieldTypeResolver.GetFieldValueType(templateField);
            if (valueType != null)
            {
                yield return GenerateFieldValueProperty(model.Template, templateField, valueType);
            }
        }

        private PropertyDeclarationSyntax GenerateFieldProperty(Template template, TemplateField templateField)
        {
            try
            {
                return PropertyDeclaration(ParseTypeName(_fieldTypeResolver.GetFieldInterfaceType(templateField)), _fieldNameResolver.GetFieldName(templateField))
                       .AddAccessorListAccessors(AutoGet())
                       .AddSingleAttributes(SitecoreIndexField(templateField.Name))
                       .WithLeadingTrivia(_xmlDocGenerator.GetFieldComment(template, templateField));
            }
            catch (Exception ex)
            {
                throw new GenerationException($"Could not generate field value property for field {templateField.Name} on template {template.Name}.", ex);
            }
        }

        private MemberDeclarationSyntax[] GenerateFields(ModelType model, IImmutableList<TemplateField> templateOwnFields)
        {
            return templateOwnFields
                   .SelectMany(f => GenerateField(model, f))
                   .ToArray();
        }

        private PropertyDeclarationSyntax GenerateFieldValueProperty(Template template, TemplateField templateField, string valueType)
        {
            try
            {
                return PropertyDeclaration(ParseTypeName(valueType), _fieldNameResolver.GetFieldValueName(templateField))
                       .AddAccessorListAccessors(AutoGet())
                       .AddSingleAttributes(SitecoreIndexField(templateField.Name))
                       .WithLeadingTrivia(_xmlDocGenerator.GetFieldComment(template, templateField));
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