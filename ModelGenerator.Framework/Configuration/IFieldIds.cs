using System;

namespace ModelGenerator.Framework.FileParsing
{
    public interface IFieldIds
    {
        Guid BaseTemplates { get; init; }
        Guid DisplayName { get; init; }
        Guid FieldType { get; init; }
        Guid Title { get; init; }
    }
}