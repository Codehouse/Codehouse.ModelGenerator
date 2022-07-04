using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ModelGenerator.Framework.CodeGeneration
{
    /// <summary>
    ///     <para>This rewriter applies itself in two main places: properties and fields.</para>
    ///     <para>
    ///         With fields, the declarations are all simple and do not require interstitial spacing.
    ///         Instead, this simply ensures that there is a line of whitespace separating the fields from
    ///         whichever members are declared next.
    ///     </para>
    ///     <para>
    ///         With properties, the default normalisation does not consistently introduce spacing
    ///         between properties, which typically are annotated with attributes and also XmlDoc blocks.
    ///         This class ensures that there are two EOL syntax trivia elements trailing each property
    ///         declaration, provided that it is not the last member of a class.
    ///     </para>
    /// </summary>
    public class SpacingRewriter : CSharpSyntaxRewriter, IRewriter
    {
        public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            // Add a line break between the last field and following members.
            var typeDeclaration = node.Parent as TypeDeclarationSyntax;
            if (typeDeclaration != null && node == typeDeclaration.Members.OfType<FieldDeclarationSyntax>().Last())
            {
                node = node.WithTrailingTrivia(SyntaxHelper.NewLineTrivia(2));
            }

            return base.VisitFieldDeclaration(node);
        }

        public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (node.Parent is TypeDeclarationSyntax parentType)
            {
                // Always have line break between properties
                var isLast = node == parentType.Members.Last();
                if (!isLast)
                {
                    var trivia = node.GetTrailingTrivia();
                    if (trivia.Count(t => t.IsKind(SyntaxKind.EndOfLineTrivia)) == 1)
                    {
                        trivia = trivia.Add(SyntaxHelper.NewLineTrivia());
                        node = node.WithTrailingTrivia(trivia);
                    }
                }
            }

            return base.VisitPropertyDeclaration(node);
        }
    }
}