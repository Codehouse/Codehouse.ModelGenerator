using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace ModelGenerator.Framework.ItemModelling
{
    [DebuggerDisplay("TemplateSet: {Name} {Id}")]
    public class TemplateSet
    {
        public string Id { get; init; }
        public string ItemPath { get; init; }
        public string ModelPath { get; init; }
        public string Name { get; init; }
        public ImmutableArray<string> References { get; set; }
        public IImmutableDictionary<Guid, Template> Templates { get; init; }
    }
}