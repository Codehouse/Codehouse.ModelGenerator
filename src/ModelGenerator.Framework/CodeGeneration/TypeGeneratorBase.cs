using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Framework.CodeGeneration.FileTypes;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration
{
    public abstract class TypeGeneratorBase<TFile> : ITypeGenerator<TFile>
        where TFile : IFileType
    {
        /// <inheritdoc />
        public abstract string Tag { get; }

        /// <inheritdoc />
        public NamespacedType? GenerateType(TFile file)
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

        protected abstract (string? Name, TypeDeclarationSyntax? Class) GenerateTypeDeclaration(ScopedRagBuilder<string> statusTracker, GenerationContext context, ModelFile model);

        protected abstract string GetNamespace(GenerationContext context, ModelType modelType);
    }
}