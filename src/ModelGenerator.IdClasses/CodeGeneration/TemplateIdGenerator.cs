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
    public class TemplateIdGenerator : IdTypeGeneratorBase 
    {
        public override string Tag => "Ids.Template";

        private readonly IdSettings _settings;
        private readonly ITypeNameResolver _typeNameResolver;
        private readonly IXmlDocumentationGenerator _xmlDocGenerator;

        public TemplateIdGenerator(IdSettings settings, ITypeNameResolver typeNameResolver, IXmlDocumentationGenerator xmlDocGenerator)
            : base(settings, typeNameResolver)
        {
            _settings = settings;
            _typeNameResolver = typeNameResolver;
            _xmlDocGenerator = xmlDocGenerator;
        }

        protected override (string? Name, TypeDeclarationSyntax? Class) GenerateTypeDeclaration(ScopedRagBuilder<string> statusTracker, GenerationContext context, ModelFile model)
        {
            var properties = model.Types
                                  .Select(m => GenerateIdProperty(m.Template))
                                  .ToArray();

            var type = SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(_settings.TemplateIdsTypeName))
                                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                                    .AddMembers(properties);
            return (_settings.TemplateIdsTypeName, type);
        }

        private PropertyDeclarationSyntax GenerateIdProperty(Template template)
        {
            return GenerateIdProperty(_typeNameResolver.GetTypeName(template), template.Id)
               .WithLeadingTrivia(_xmlDocGenerator.GetTemplateComment(template));
        }
    }
}