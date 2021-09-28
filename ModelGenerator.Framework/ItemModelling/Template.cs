using System;
using System.Collections.Immutable;
using System.Diagnostics;
using ModelGenerator.Framework.FileParsing;

namespace ModelGenerator.Framework.ItemModelling
{
    [DebuggerDisplay("Template: {Name} ({Id})")]
    public record Template
    {
        public Guid[] BaseTemplateIds { get; init; }
        public string DisplayName { get; init; }
        public Guid Id { get; init; }
        public Item Item { get; init; }
        public string Name { get; init; }
        public IImmutableList<TemplateField> OwnFields { get; init; }
        public string Path { get; init; }
        public string SetId { get; init; }
    }
}