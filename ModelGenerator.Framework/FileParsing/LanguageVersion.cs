using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace ModelGenerator.Framework.FileParsing
{
    [DebuggerDisplay("{Language}#{Number}")]
    public record LanguageVersion
    {
        public int Number { get; init; }
        public string Language { get; init; }
        public Guid Revision { get; init; }

        public ImmutableDictionary<Guid, Field> Fields { get; init; }
    }
}