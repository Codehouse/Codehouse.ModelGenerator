using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FortisInterfaceGenerator : IGenerator<ModelInterface, MemberDeclarationSyntax>
    {
        private const string FortisModelTemplateMappingAttribute = "Fortis.Model.TemplateMapping";
        private const string SitecoreIndexFieldAttribute = "Sitecore.ContentSearch.IndexField";
        private readonly TypeNameGenerator _typeNameGeneratorm;
        private readonly XmlDocGenerator _xmlDocGenerator;

        public FortisInterfaceGenerator(TypeNameGenerator typeNameGeneratorm, XmlDocGenerator xmlDocGenerator)
        {
            _typeNameGeneratorm = typeNameGeneratorm;
            _xmlDocGenerator = xmlDocGenerator;
        }
        
        public IEnumerable<MemberDeclarationSyntax> GenerateCode(GenerationContext context, ModelInterface model)
        {
            // TODO: Base interface types
            var type = SyntaxFactory.InterfaceDeclaration(_typeNameGeneratorm.GetInterfaceName(model.Template))
                                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                    .AddBaseListTypes(GenerateBaseTypes(context, model.Template))
                                    .AddSingleAttributes(
                                        SyntaxFactory.Attribute(SyntaxFactory.ParseName(FortisModelTemplateMappingAttribute))
                                                     .AddArgumentListArguments(
                                                         SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(model.Template.Id.ToString("B").ToUpperInvariant()))),
                                                         SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("InterfaceMap")))
                                                     )
                                    )
                                    .AddMembers(GenerateMembers(model, model.Template.OwnFields))
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
                           .Select(typeName => SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(typeName)))
                           .ToArray();
        }

        private string GetBaseTypeName(Template currentTemplate, Template baseTemplate, TemplateCollection collection)
        {
            var baseTemplateSet = collection.TemplateSets[baseTemplate.SetId];
            return _typeNameGeneratorm.GetRelativeInterfaceName(currentTemplate, baseTemplate, baseTemplateSet);
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateMember(ModelInterface model, TemplateField templateField)
        {
            // TODO: Resolve field type to Fortis type
            yield return SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("string"), templateField.Name)
                                      .AddAccessorListAccessors(
                                          SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                                       .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))
                                      .AddSingleAttributes(
                                          SyntaxFactory.Attribute(SyntaxFactory.ParseName(SitecoreIndexFieldAttribute))
                                                       .AddArgumentListArguments(
                                                           SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(templateField.Name.ToLowerInvariant())))
                                                       )
                                      )
                                      .WithLeadingTrivia(_xmlDocGenerator.GenerateFieldComment(model.Template, templateField));
            
            // TODO: Resolve field type to value type
            yield return SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("string"), templateField.Name + "Value")
                                      .AddAccessorListAccessors(
                                          SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                                       .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))
                                      .AddSingleAttributes(
                                          SyntaxFactory.Attribute(SyntaxFactory.ParseName(SitecoreIndexFieldAttribute))
                                                       .AddArgumentListArguments(
                                                           SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(templateField.Name.ToLowerInvariant())))
                                                       )
                                      )
                                      .WithLeadingTrivia(_xmlDocGenerator.GenerateFieldComment(model.Template, templateField));
        }

        private MemberDeclarationSyntax[] GenerateMembers(ModelInterface model, IImmutableList<TemplateField> templateOwnFields)
        {
            return templateOwnFields
                   .SelectMany(f => GenerateMember(model, f))
                   .ToArray();
        }
    }
}