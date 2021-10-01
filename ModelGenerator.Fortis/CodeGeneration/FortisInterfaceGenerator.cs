﻿using System.Collections.Generic;
using System.Collections.Immutable;
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
    public class FortisInterfaceGenerator : IGenerator<ModelInterface, MemberDeclarationSyntax>
    {
        private readonly FieldNameResolver _fieldNameResolver;
        private readonly FieldTypeResolver _fieldTypeResolver;
        private readonly TypeNameGenerator _typeNameGenerator;
        private readonly XmlDocGenerator _xmlDocGenerator;
        private const string FortisModelTemplateMappingAttribute = "Fortis.Model.TemplateMapping";

        public FortisInterfaceGenerator(FieldNameResolver fieldNameResolver, FieldTypeResolver fieldTypeResolver, TypeNameGenerator typeNameGenerator, XmlDocGenerator xmlDocGenerator)
        {
            _fieldNameResolver = fieldNameResolver;
            _fieldTypeResolver = fieldTypeResolver;
            _typeNameGenerator = typeNameGenerator;
            _xmlDocGenerator = xmlDocGenerator;
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateCode(GenerationContext context, ModelInterface model)
        {
            var type = InterfaceDeclaration(_typeNameGenerator.GetInterfaceName(model.Template))
                       .AddModifiers(Token(SyntaxKind.PublicKeyword))
                       .AddBaseListTypes(GenerateBaseTypes(context, model.Template))
                       .AddSingleAttributes(
                           Attribute(ParseName(FortisModelTemplateMappingAttribute))
                               .AddSimpleArguments(IdLiteral(model.Template.Id), StringLiteral("InterfaceMap"))
                       )
                       .AddMembers(GenerateFields(model, model.Template.OwnFields))
                       .WithLeadingTrivia(_xmlDocGenerator.GenerateInterfaceComment(model.Template));

            yield return type;
        }

        private SimpleBaseTypeSyntax[] GenerateBaseTypes(GenerationContext context, Template template)
        {
            var templates = context.Templates.Templates;
            return template.BaseTemplateIds
                           .Where(id => templates.ContainsKey(id))
                           .Where(id => templates[id].SetId != null)
                           .Select(id => GetBaseTypeName(template, templates[id], context.Templates))
                           .Prepend("IItem")
                           .Select(typeName => SimpleBaseType(ParseTypeName(typeName)))
                           .ToArray();
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateField(ModelInterface model, TemplateField templateField)
        {
            yield return PropertyDeclaration(ParseTypeName(_fieldTypeResolver.GetFieldInterfaceType(templateField)), _fieldNameResolver.GetFieldName(templateField))
                         .AddAccessorListAccessors(AutoGet())
                         .AddSingleAttributes(SitecoreIndexField(templateField.Name))
                         .WithLeadingTrivia(_xmlDocGenerator.GenerateFieldComment(model.Template, templateField));

            var valueType = _fieldTypeResolver.GetFieldValueType(templateField);
            if (valueType != null)
            {
                yield return PropertyDeclaration(ParseTypeName(valueType), _fieldNameResolver.GetFieldValueName(templateField))
                             .AddAccessorListAccessors(AutoGet())
                             .AddSingleAttributes(SitecoreIndexField(templateField.Name))
                             .WithLeadingTrivia(_xmlDocGenerator.GenerateFieldComment(model.Template, templateField));
            }
        }

        private MemberDeclarationSyntax[] GenerateFields(ModelInterface model, IImmutableList<TemplateField> templateOwnFields)
        {
            return templateOwnFields
                   .SelectMany(f => GenerateField(model, f))
                   .ToArray();
        }

        private string GetBaseTypeName(Template currentTemplate, Template baseTemplate, TemplateCollection collection)
        {
            var baseTemplateSet = collection.TemplateSets[baseTemplate.SetId];
            return _typeNameGenerator.GetRelativeInterfaceName(currentTemplate, baseTemplate, baseTemplateSet);
        }
    }
}