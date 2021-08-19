using System;

namespace ModelGenerator.Framework.FileParsing
{
    public record LanguageVersion
    {
        public int Number { get; init; }
        public string Language { get; init; }
        public Guid Revision { get; init; }
    }
}