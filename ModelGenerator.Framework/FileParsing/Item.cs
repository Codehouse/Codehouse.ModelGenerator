using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace ModelGenerator.Framework.FileParsing
{
    [DebuggerDisplay("{Name} {Id}")]
    public record Item
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public Guid Parent { get; init; }
        public string RawFilePath { get; init; }
        public string Path { get; init; }
        public string TemplateName { get; init; }
        public Guid TemplateId { get; init; }
        
        public ImmutableList<Field> SharedFields { get; init; }
        public ImmutableList<LanguageVersion> Versions { get; init; }
    }
}