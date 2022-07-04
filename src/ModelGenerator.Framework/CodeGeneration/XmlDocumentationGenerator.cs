using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ModelGenerator.Framework.Configuration;
using ModelGenerator.Framework.ItemModelling;
using SuperXML;

namespace ModelGenerator.Framework.CodeGeneration
{
    public class XmlDocumentationGenerator : IXmlDocumentationGenerator
    {
        private readonly XmlDocumentationSettings _settings;

        public XmlDocumentationGenerator(XmlDocumentationSettings settings)
        {
            _settings = settings;
            ConfigureCompiler();
        }

        public SyntaxTriviaList GetFieldComment(Template template, TemplateField field)
        {
            // TODO: SuperXML might have a memory leak - consider switching templating.
            var xml = new Compiler()
                     .AddKey("t", template)
                     .AddKey("f", field)
                     .CompileString(_settings.Field);
            return SyntaxFactory.ParseLeadingTrivia(xml + Environment.NewLine);
        }

        public SyntaxTriviaList GetTemplateComment(Template template)
        {
            var xml = new Compiler()
                     .AddKey("t", template)
                     .CompileString(_settings.Template);
            return SyntaxFactory.ParseLeadingTrivia(xml + Environment.NewLine);
        }

        private void ConfigureCompiler()
        {
            Compiler.Filters.Add("scid", ToSitecoreIdFilter);
        }

        private string ToSitecoreIdFilter(object arg)
        {
            if (arg is Guid id)
            {
                return id.ToSitecoreId();
            }

            throw new NotSupportedException($"{nameof(ToSitecoreIdFilter)} cannot be used with type {arg.GetType().Name}.");
        }
    }
}