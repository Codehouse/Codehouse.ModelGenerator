using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace ModelGenerator.Framework.FileParsing
{
    [DebuggerDisplay("Version: {Language}#{Number}")]
    public record LanguageVersion(ImmutableDictionary<Guid, Field> Fields, string Language, int Number, Guid Revision);
}