using System;
using System.Diagnostics;
using ModelGenerator.Framework.FileParsing;

namespace ModelGenerator.Framework.ItemModelling
{
    [DebuggerDisplay("TemplateField: {Name} {Id}")]
    public record TemplateField(
        string DisplayName,
        string FieldType,
        Guid Id,
        Item Item,
        string Name,
        string SectionName,
        Guid TemplateId
    );
}