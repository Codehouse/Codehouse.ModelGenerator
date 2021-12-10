using System.Collections.Immutable;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Framework.TypeConstruction
{
    public record TypeSet
    {
        public IImmutableList<ModelFile> Files { get; init; } = ImmutableList<ModelFile>.Empty;
        public string Name { get; init; } = string.Empty;
        public string Namespace { get; init; } = string.Empty;
        public ImmutableArray<TemplateSet> References { get; set; } = ImmutableArray<TemplateSet>.Empty;
        public string RootPath { get; init; } = string.Empty;
    }
}