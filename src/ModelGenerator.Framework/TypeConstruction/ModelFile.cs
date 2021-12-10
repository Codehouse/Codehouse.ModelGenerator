using System.Collections.Immutable;

namespace ModelGenerator.Framework.TypeConstruction
{
    public record ModelFile(
        string FileName,
        string RootPath,
        IImmutableList<ModelType> Types);
}