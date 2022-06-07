using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Fortis.Configuration;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FortisTemplateIdGenerator : FortisIdGeneratorBase
    {
        public override string Tag => "Fortis.TemplateIds";
        
        private readonly TypeNameResolver _typeNameResolver;
        private readonly IXmlDocumentationGenerator _xmlDocGenerator;
        private const string ClassName = "TemplateIds";

        public FortisTemplateIdGenerator(FortisSettings settings, TypeNameResolver typeNameResolver, IXmlDocumentationGenerator xmlDocGenerator) : base(settings, typeNameResolver)
        {
            _typeNameResolver = typeNameResolver;
            _xmlDocGenerator = xmlDocGenerator;
        }

        protected override (string? Name, TypeDeclarationSyntax? Class) GenerateTypeDeclaration(ScopedRagBuilder<string> statusTracker, GenerationContext context, ModelFile model)
        {
            var properties = model.Types
                .Select(m => GenerateIdProperty(m.Template))
                .ToArray();

            var type = SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(ClassName))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(properties);
            return (ClassName, type);
        }

        private PropertyDeclarationSyntax GenerateIdProperty(Template template)
        {
            return GenerateIdProperty(_typeNameResolver.GetTypeName(template), template.Id)
                .WithLeadingTrivia(_xmlDocGenerator.GetTemplateComment(template));
        }
    }
}