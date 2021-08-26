using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace ModelGenerator.Framework.ItemModelling
{
    [DebuggerDisplay("Template {Name} ({Id})")]
    public record Template
    {
        public Guid[] BaseTemplateIds { get; init; }
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string DisplayName { get; init; }
        public IImmutableList<TemplateField> OwnFields { get; init; }
    }
}