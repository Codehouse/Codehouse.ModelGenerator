using System;

namespace ModelGenerator.Framework.FileParsing
{
    public record Field
    {
        public Guid Id { get; init; }
        public string Value { get; init; }
    }
}