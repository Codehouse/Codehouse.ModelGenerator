using Microsoft.CodeAnalysis;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Framework.CodeGeneration
{
    public interface IXmlDocumentationGenerator
    {
        SyntaxTriviaList GetFieldComment(Template template, TemplateField field);

        SyntaxTriviaList GetTemplateComment(Template template);
    }
}