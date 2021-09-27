using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace ModelGenerator.Framework.FileParsing
{
    [DebuggerDisplay("Version: {Language}#{Number}")]
    public record LanguageVersion
    {
        public ImmutableDictionary<Guid, Field> Fields { get; init; }
        public string Language { get; init; }
        public int Number { get; init; }
        public Guid Revision { get; init; }
    }
}