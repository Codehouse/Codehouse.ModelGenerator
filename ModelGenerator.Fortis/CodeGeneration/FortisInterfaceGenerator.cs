using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FortisInterfaceGenerator : IGenerator<ModelInterface, MemberDeclarationSyntax>
    {
        public IEnumerable<MemberDeclarationSyntax> GenerateCode(GenerationContext context, ModelInterface model)
        {
            var type = SyntaxFactory.InterfaceDeclaration(model.Name)
                                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                    //.AddBaseListTypes()
                                    .AddAttributeLists(
                                        SyntaxFactory.AttributeList()
                                                     .AddAttributes(
                                                         SyntaxFactory.Attribute(SyntaxFactory.ParseName("Fortis.Model.TemplateMapping"))
                                                                      .AddArgumentListArguments(
                                                                          SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(model.Template.Id.ToString("B").ToUpperInvariant()))),
                                                                          SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(model.Template.Name)))
                                                                      )
                                                     )
                                    )
                                    .AddMembers(GenerateMembers(model, model.Template.OwnFields));

            yield return type;
        }

        private MemberDeclarationSyntax[] GenerateMembers(ModelInterface model, IImmutableList<TemplateField> templateOwnFields)
        {
            return templateOwnFields
                   .Select(f => GenerateMember(model, f))
                   .ToArray();
        }

        private MemberDeclarationSyntax GenerateMember(ModelInterface model, TemplateField templateField)
        {
            return SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("string"), templateField.Name)
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                .AddAccessorListAccessors(
                                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                                 .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                                 .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
            
        }
    }
}