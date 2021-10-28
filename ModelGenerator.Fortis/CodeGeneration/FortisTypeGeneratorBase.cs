using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.ItemModelling;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static ModelGenerator.Framework.CodeGeneration.SyntaxHelper;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public abstract class FortisTypeGeneratorBase
    {
        private const string FortisModelTemplateMappingAttribute = "TemplateMapping";
        
        protected AttributeSyntax CreateTemplateMappingAttribute(Guid templateId, string mappingType)
        {
            return Attribute(ParseName(FortisModelTemplateMappingAttribute))
                .AddSimpleArguments(IdLiteral(templateId), StringLiteral(mappingType));
        }
        
        /// <summary>
        /// Creates the appropriate GetField<T> invocation for a template field.
        /// </summary>
        /// <param name="isRenderingParameter">A value indicating whether the template is a rendering parameters template.</param>
        /// <param name="templateField">The template field</param>
        /// <param name="concreteType">The concrete type of the field</param>
        /// <returns></returns>
        protected InvocationExpressionSyntax GetFieldInvocation(bool isRenderingParameter, TemplateField templateField, string concreteType)
        {
            var arguments = Enumerable.Empty<ArgumentSyntax>()
                                      .Append(Argument(StringLiteral(templateField.Name)));
            if (!isRenderingParameter)
            {
                arguments = arguments.Append(Argument(StringLiteral(templateField.Name.ToLowerInvariant())));
            }
            
            // GetField<TField>("FieldName", "fieldname") for normal templates
            // GetField<TField>("FieldName") for rendering parameter templates
            return InvocationExpression(
                    GenericName(Identifier("GetField"))
                        .AddTypeArgumentListArguments(IdentifierName(concreteType))
                )
                .AddArgumentListArguments(arguments.ToArray());
        }
    }
}