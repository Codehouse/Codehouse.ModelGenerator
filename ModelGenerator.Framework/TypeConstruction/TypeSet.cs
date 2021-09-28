using System.Collections.Immutable;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Framework.TypeConstruction
{
    public record TypeSet
    {
        public IImmutableList<ModelFile> Files { get; init; }
        public string Name { get; init; }
        public ImmutableArray<TemplateSet> References { get; set; }
    }
}