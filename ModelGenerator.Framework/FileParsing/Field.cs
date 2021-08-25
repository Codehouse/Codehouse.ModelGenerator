using System;
using System.Diagnostics;

namespace ModelGenerator.Framework.FileParsing
{
    [DebuggerDisplay("{Name} {Id}")]
    public record Field
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string Value { get; init; }
    }
}