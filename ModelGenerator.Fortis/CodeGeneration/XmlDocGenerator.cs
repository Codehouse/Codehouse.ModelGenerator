using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class XmlDocGenerator
    {
        /// <summary>
        /// Requires arguments:
        /// <list type="number">
        /// <item>Template name</item>
        /// <item>Field name</item>
        /// <item>Field type</item>
        /// </list>
        /// </summary>
        private const string _fieldComment = @"/// <summary>
/// <para>Template: {0}</para>
/// <para>Field: {1}</para>
/// <para>Data type: {2}</para>
/// </summary>
";
        
        /// <summary>
        /// Requires arguments:
        /// <list type="number">
        /// <item>Template name</item>
        /// <item>Template id</item>
        /// <item>Template path</item>
        /// </list>
        /// </summary>
        private const string _interfaceComment = @"/// <summary>
/// <para>Template interface</para>
/// <para>Template: {0}</para>
/// <para>ID: {1}</para>
/// <para>Path: {2}</para>
/// </summary>
";
        
        /// <summary>
        /// Requires arguments:
        /// <list type="number">
        /// <item>Template path</item>
        /// </list>
        /// </summary>
        private const string _classComment = @"/// <summary>
/// <para>Template class</para>
/// <para>Path: {0}</para>
/// </summary>
";
        
        public SyntaxTriviaList GenerateClassComment(Template template)
        {
            return SyntaxFactory.ParseLeadingTrivia(string.Format(_classComment, template.Path));
        }
        
        public SyntaxTriviaList GenerateFieldComment(Template template, TemplateField field)
        {
            return SyntaxFactory.ParseLeadingTrivia(string.Format(_fieldComment, template.Name, field.Name, field.FieldType));
        }
        
        public SyntaxTriviaList GenerateInterfaceComment(Template template)
        {
            return SyntaxFactory.ParseLeadingTrivia(string.Format(_interfaceComment, template.Name, template.Id.ToString("B"), template.Path));
        }
    }
}