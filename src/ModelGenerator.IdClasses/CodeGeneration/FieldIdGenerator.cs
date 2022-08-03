using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;
using ModelGenerator.IdClasses.Configuration;

namespace ModelGenerator.IdClasses.CodeGeneration
{
    public class FieldIdGenerator : IdTypeGeneratorBase
    {
        public override string Tag => "Ids.Field";

        private readonly IFieldNameResolver _fieldNameResolver;
        private readonly IdSettings _settings;
        private readonly ITypeNameResolver _typeNameResolver;
        private readonly IXmlDocumentationGenerator _xmlDocGenerator;

        public FieldIdGenerator(IFieldNameResolver fieldNameResolver, IdSettings settings, ITypeNameResolver typeNameResolver, IXmlDocumentationGenerator xmlDocGenerator)
            : base(settings, typeNameResolver)
        {
            _fieldNameResolver = fieldNameResolver;
            _settings = settings;
            _typeNameResolver = typeNameResolver;
            _xmlDocGenerator = xmlDocGenerator;
        }

        protected override (string? Name, TypeDeclarationSyntax? Class) GenerateTypeDeclaration(ScopedRagBuilder<string> statusTracker, GenerationContext context, ModelFile model)
        {
            if (!model.Types.SelectMany(m => m.Template.OwnFields).Any())
            {
                return (null, null);
            }

            return (_settings.FieldIdsTypeName, GenerateFieldIdClasses(statusTracker, context, model.Types));
        }

        private TypeDeclarationSyntax GenerateFieldIdClasses(ScopedRagBuilder<string> ragBuilder, GenerationContext context, IEnumerable<ModelType> models)
        {
            var innerClasses = models
                              .Select(m => GenerateFieldIdInnerClass(ragBuilder, context, m))
                              .ToArray();

            return SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(_settings.FieldIdsTypeName))
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                                .AddMembers(innerClasses);
        }

        private TypeDeclarationSyntax GenerateFieldIdInnerClass(ScopedRagBuilder<string> ragBuilder, GenerationContext context, ModelType model)
        {
            var typeName = _typeNameResolver.GetTypeName(model.Template);
            var fieldProperties = model.Template.OwnFields
                                       .Select(p => GenerateIdProperty(ragBuilder, model.Template, p, typeName))
                                       .ToArray();

            return SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(typeName))
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                                .AddMembers(fieldProperties)
                                .WithLeadingTrivia(_xmlDocGenerator.GetTemplateComment(model.Template));
        }

        private PropertyDeclarationSyntax GenerateIdProperty(ScopedRagBuilder<string> ragBuilder, Template template, TemplateField field, string typeName)
        {
            var propertyName = _fieldNameResolver.GetFieldName(field);
            if (propertyName.Equals(typeName))
            {
                ragBuilder.AddWarn($"Template contains field with same name as generated field ID class ({typeName}).");
                propertyName += "FieldId";
            }

            return GenerateIdProperty(propertyName, field.Id)
               .WithLeadingTrivia(_xmlDocGenerator.GetFieldComment(template, field));
        }
    }
}