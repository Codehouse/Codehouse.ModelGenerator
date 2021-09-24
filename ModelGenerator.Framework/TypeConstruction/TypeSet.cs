using System.Collections.Immutable;

namespace ModelGenerator.Framework.TypeConstruction
{
    public record TypeSet
    {
        public string Name { get; init; }
        public IImmutableList<ModelFile> Files { get; init; }
    }
}