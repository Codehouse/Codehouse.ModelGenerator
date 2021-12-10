using System.Collections.Immutable;

namespace ModelGenerator.Framework.FileScanning
{
    public record FileSet(
        IImmutableList<ItemFile> Files,
        string Id,
        string ItemPath,
        string ModelPath,
        string Name,
        string Namespace,
        ImmutableArray<string> References
    );
}