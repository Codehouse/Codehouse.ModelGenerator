using System.Collections.Immutable;

namespace ModelGenerator.Framework.TypeConstruction
{
    public record ModelFile
    {
        public string FileName { get; init; }
        public string Namespace { get; init; }
        public string Path { get; init; }
        public IImmutableList<ModelType> Types { get; init; }
    }
}