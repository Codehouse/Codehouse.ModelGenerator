using System.Collections.Immutable;

namespace ModelGenerator.Framework.TypeConstruction
{
    public record ModelFile
    {
        public string FileName { get; init; }
        public string RootPath { get; init; }
        public IImmutableList<ModelType> Types { get; init; }
    }
}