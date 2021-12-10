using System;
using System.Diagnostics;

namespace ModelGenerator.Framework.FileParsing
{
    [DebuggerDisplay("Field: {Name} {Id}")]
    public record Field(
        Guid Id,
        string Name,
        string Value
    );
}