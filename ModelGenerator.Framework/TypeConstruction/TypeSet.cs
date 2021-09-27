using System.Collections.Immutable;

namespace ModelGenerator.Framework.TypeConstruction
{
    public record TypeSet
    {
        public IImmutableList<ModelFile> Files { get; init; }
        public string Name { get; init; }
    }
}