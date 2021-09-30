using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ModelGenerator.Framework.CodeGeneration
{
    public static class SyntaxExtensions
    {
        public static AccessorDeclarationSyntax AddSingleAttributes(this AccessorDeclarationSyntax member, params AttributeSyntax[] attributes)
        {
            var attributeLists = attributes
                                 .Select(a => SyntaxFactory.AttributeList().AddAttributes(a))
                                 .ToArray();
            return member.AddAttributeLists(attributeLists);
        }
        
        public static ClassDeclarationSyntax AddSingleAttributes(this ClassDeclarationSyntax member, params AttributeSyntax[] attributes)
        {
            var attributeLists = attributes
                                 .Select(a => SyntaxFactory.AttributeList().AddAttributes(a))
                                 .ToArray();
            return member.AddAttributeLists(attributeLists);
        }
        
        public static MemberDeclarationSyntax AddSingleAttributes(this MemberDeclarationSyntax member, params AttributeSyntax[] attributes)
        {
            var attributeLists = attributes
                                 .Select(a => SyntaxFactory.AttributeList().AddAttributes(a))
                                 .ToArray();
            return member.AddAttributeLists(attributeLists);
        }
        
        public static InterfaceDeclarationSyntax AddSingleAttributes(this InterfaceDeclarationSyntax member, params AttributeSyntax[] attributes)
        {
            var attributeLists = attributes
                                 .Select(a => SyntaxFactory.AttributeList().AddAttributes(a))
                                 .ToArray();
            return member.AddAttributeLists(attributeLists);
        }
    }
}