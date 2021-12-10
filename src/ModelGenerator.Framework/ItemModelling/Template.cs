using System;
using System.Collections.Immutable;
using System.Diagnostics;
using ModelGenerator.Framework.FileParsing;

namespace ModelGenerator.Framework.ItemModelling
{
    [DebuggerDisplay("Template: {Name} ({Id})")]
    public record Template(
        Guid[] BaseTemplateIds,
        string DisplayName,
        Guid Id,
        bool IsWellKnown,
        Item? Item,
        string LocalNamespace,
        string Name,
        IImmutableList<TemplateField> OwnFields,
        string Path,
        string SetId
    );
}