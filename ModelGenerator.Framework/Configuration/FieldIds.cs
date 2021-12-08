using System;

namespace ModelGenerator.Framework.FileParsing
{
    public record FieldIds
    {
        public Guid BaseTemplates { get; init; }
        public Guid DisplayName { get; init; }
        public Guid FieldType { get; init; }
        public Guid Title { get; init; }
    }
}