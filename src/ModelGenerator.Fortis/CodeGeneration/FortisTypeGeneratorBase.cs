using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Fortis.Configuration;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.CodeGeneration.FileTypes;
using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static ModelGenerator.Framework.CodeGeneration.SyntaxHelper;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public abstract class FortisTypeGeneratorBase : ITypeGenerator<DefaultFile>
    {
        public abstract string Tag { get; }

        private readonly FortisSettings _settings;
        private readonly TypeNameResolver _typeNameResolver;
        private const string FortisModelTemplateMappingAttribute = "TemplateMapping";

        protected FortisTypeGeneratorBase(FortisSettings settings, TypeNameResolver typeNameResolver)
        {
            _settings = settings;
            _typeNameResolver = typeNameResolver;
        }

        public NamespacedType? GenerateType(DefaultFile file)
        {
            var (name, @class) = GenerateTypeDeclaration(file.ScopedRagBuilder, file.Context, file.Model);
            if (string.IsNullOrEmpty(name) || @class is null)
            {
                return null;
            }

            return new NamespacedType(
                GetNamespace(file.Context, file.Model.Types.First()),
                @class,
                name);
        }

        protected AttributeSyntax CreateTemplateMappingAttribute(Guid templateId, string mappingType)
        {
            return Attribute(ParseName(FortisModelTemplateMappingAttribute))
               .AddSimpleArguments(IdLiteral(templateId), StringLiteral(mappingType));
        }

        protected abstract (string? Name, TypeDeclarationSyntax? Class) GenerateTypeDeclaration(ScopedRagBuilder<string> statusTracker, GenerationContext context, ModelFile model);

        /// <summary>
        ///     Creates the appropriate GetField<T> or (T)GetField invocation for a template field.
        /// </summary>
        /// <param name="isRenderingParameter">A value indicating whether the template is a rendering parameters template.</param>
        /// <param name="templateField">The template field</param>
        /// <param name="concreteType">The concrete type of the field</param>
        /// <returns></returns>
        protected ExpressionSyntax GetFieldInvocation(bool isRenderingParameter, TemplateField templateField, string concreteType)
        {
            var arguments = Enumerable.Empty<ArgumentSyntax>()
                                      .Append(Argument(StringLiteral(templateField.Name)));
            if (!isRenderingParameter)
            {
                arguments = arguments.Append(Argument(StringLiteral(templateField.Name.ToLowerInvariant())));
            }

            if (isRenderingParameter && _settings.Quirks.CastRenderingParameterFields)
            {
                arguments = arguments.Append(Argument(StringLiteral(templateField.FieldType.ToLowerInvariant())));

                // (TField)GetField("FieldName", "fieldtype") for rendering parameter templates with this quirk
                return CastExpression(
                    IdentifierName(concreteType),
                    InvocationExpression(IdentifierName("GetField"))
                       .AddArgumentListArguments(arguments.ToArray())
                );
            }

            // GetField<TField>("FieldName", "fieldname") for normal templates
            // GetField<TField>("FieldName") for rendering parameter templates
            return InvocationExpression(
                    GenericName(Identifier("GetField"))
                       .AddTypeArgumentListArguments(IdentifierName(concreteType))
                )
               .AddArgumentListArguments(arguments.ToArray());
        }

        protected virtual string GetNamespace(GenerationContext context, ModelType modelType)
        {
            return _typeNameResolver.GetNamespace(context.TypeSet, modelType.Template);
        }
    }
}