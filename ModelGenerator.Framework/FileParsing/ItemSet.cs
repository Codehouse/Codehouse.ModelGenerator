using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace ModelGenerator.Framework.FileParsing
{
    [DebuggerDisplay("ItemSet: {Name} {Id}")]
    public class ItemSet
    {
        public string Id { get; init; }
        public string ItemPath { get; init; }
        public IImmutableDictionary<Guid, Item> Items { get; init; }
        public string ModelPath { get; init; }
        public string Name { get; init; }
    }
}