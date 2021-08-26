using System;

namespace ModelGenerator.Framework.ItemModelling
{
    public record TemplateField
    {
        public string DisplayName { get; init; }
        public string FieldType { get; init; }
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string SectionName { get; init; }
    }
}