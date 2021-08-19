using System;
using System.Collections.Immutable;

namespace ModelGenerator.Framework.FileParsing
{
    public class ItemSet
    {
        public string Id { get; init; }
        public IImmutableDictionary<Guid, Item> Items { get; init; }
        public string ItemPath { get; init; }
        public string ModelPath { get; init; }
        public string Name { get; init; }
    }
}