using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ModelGenerator.Framework.CodeGeneration
{
    /// <summary>
    ///     <para>
    ///         This rewriter splits base type lists with more than <see cref="_baseListLengthLimit" />
    ///         items so that each class after the first appears on a new line.
    ///     </para>
    ///     <para>
    ///         Each line is indented <see cref="_baseListIndentation" /> further than the outer type
    ///         declaration.
    ///     </para>
    /// </summary>
    public class BaseTypeListRewriter : CSharpSyntaxRewriter, IRewriter
    {
        private int _indent;
        private const int _baseListIndentation = 4;
        private const int _baseListLengthLimit = 2;

        public override SyntaxNode? VisitBaseList(BaseListSyntax node)
        {
            if (node.Parent is TypeDeclarationSyntax typeDeclaration)
            {
                _indent = typeDeclaration.Modifiers
                                         .First()
                                         .LeadingTrivia
                                         .Single(t => t.IsKind(SyntaxKind.WhitespaceTrivia))
                                         .Span
                                         .Length;
            }

            return base.VisitBaseList(node);
        }

        public override SyntaxNode? VisitSimpleBaseType(SimpleBaseTypeSyntax node)
        {
            if (node.Parent is BaseListSyntax baseList)
            {
                if (node != baseList.Types.First() && baseList.Types.Count > _baseListLengthLimit)
                {
                    node = node.WithLeadingTrivia(
                        SyntaxHelper.NewLineTrivia(),
                        SyntaxHelper.Whitespace(_indent + _baseListIndentation));
                }
            }

            return base.VisitSimpleBaseType(node);
        }
    }
}