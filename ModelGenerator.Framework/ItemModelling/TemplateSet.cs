using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace ModelGenerator.Framework.ItemModelling
{
    [DebuggerDisplay("TemplateSet: {Name} {Id}")]
    public record TemplateSet(
        string Id,
        string ItemPath,
        string ModelPath,
        string Name,
        string Namespace,
        ImmutableArray<string> References,
        IImmutableDictionary<Guid, Template> Templates
    );
}