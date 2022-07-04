using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ModelGenerator.Framework.CodeGeneration
{
    /// <summary>
    ///     <para>This class rewrites the generated expression-bodied property accessors.</para>
    ///     <para>By default generated accessors, which are attributed, generate as follows:</para>
    ///     <code>
    /// [Sitecore.ContentSearch.IndexField("fieldName")]
    /// public string FieldNameValue {[DebuggerStepThrough]
    ///     get => FieldName.Value; }
    /// </code>
    ///     <para>
    ///         This rewriter introduces leading whitespace trivia on the attribute list,
    ///         and trailing whitespace after the accessor declaration.  This results in the
    ///         following code:
    ///     </para>
    ///     <code>
    /// [Sitecore.ContentSearch.IndexField("description")]
    /// public string DescriptionValue {
    ///     [DebuggerStepThrough]
    ///     get => Description.Value;
    /// }
    /// </code>
    /// </summary>
    public class AccessorRewriter : CSharpSyntaxRewriter, IRewriter
    {
        public override SyntaxNode? VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            var accessorList = node.Parent as AccessorListSyntax;

            // Only add the trailing whitespace to the last accessor declaration in any given list.
            // Mostly a formality, as currently only getters are emitted.
            if (accessorList != null && node.ExpressionBody != null && node == accessorList.Accessors.Last())
            {
                var propertyDeclaration = accessorList.Parent as PropertyDeclarationSyntax;
                if (propertyDeclaration != null)
                {
                    var trivia = SyntaxTriviaList.Empty
                                                 .Add(SyntaxHelper.NewLineTrivia())
                                                 .AddRange(propertyDeclaration.Modifiers.First().LeadingTrivia.Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia)));
                    node = node.WithSemicolonToken(SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.SemicolonToken, trivia));
                }
            }

            return base.VisitAccessorDeclaration(node);
        }

        public override SyntaxNode? VisitAttributeList(AttributeListSyntax node)
        {
            var propertyAccessors = node.Parent as AccessorDeclarationSyntax;

            // Only alter whitespace leading the first attribute list.
            if (propertyAccessors != null && node == propertyAccessors.AttributeLists.First())
            {
                var trivia = node.GetLeadingTrivia()
                                 .InsertRange(0, propertyAccessors.Keyword.LeadingTrivia)
                                 .Insert(0, SyntaxHelper.NewLineTrivia());
                node = node.WithLeadingTrivia(trivia);
            }

            return base.VisitAttributeList(node);
        }
    }
}