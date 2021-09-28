using System.Collections.Immutable;

namespace ModelGenerator.Framework.FileScanning
{
    public record FileSet
    {
        public IImmutableList<string> Files { get; init; }
        public string Id { get; init; }
        public string ItemPath { get; init; }
        public string ModelPath { get; init; }
        public string Name { get; init; }
        public ImmutableArray<string> References { get; set; }
    }
}