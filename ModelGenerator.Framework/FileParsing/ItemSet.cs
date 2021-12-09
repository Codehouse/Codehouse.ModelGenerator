using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace ModelGenerator.Framework.FileParsing
{
    [DebuggerDisplay("ItemSet: {Name} {Id}")]
    public record ItemSet(
        string Id,
        string ItemPath,
        IImmutableDictionary<Guid, Item> Items,
        string ModelPath,
        string Name,
        string Namespace,
        ImmutableArray<string> References
    );
}