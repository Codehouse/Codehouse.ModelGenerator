using System;
using System.Diagnostics;
using ModelGenerator.Framework.FileParsing;

namespace ModelGenerator.Framework.ItemModelling
{
    [DebuggerDisplay("TemplateField: {Name} {Id}")]
    public record TemplateField
    {
        public string DisplayName { get; init; }
        public string FieldType { get; init; }
        public Guid Id { get; init; }
        public Item Item { get; init; }
        public string Name { get; init; }
        public string SectionName { get; init; }
        public Guid TemplateId { get; init; }
    }
}