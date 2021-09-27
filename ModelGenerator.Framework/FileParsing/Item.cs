using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace ModelGenerator.Framework.FileParsing
{
    [DebuggerDisplay("Item: {Name} {Id}")]
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

        public LanguageVersion? GetLatestVersion(string language = "en")
        {
            return Versions.Where(v => language.Equals(v.Language, StringComparison.OrdinalIgnoreCase))
                           .OrderBy(v => v.Number)
                           .LastOrDefault();
        }

        public Field? GetVersionedField(Guid fieldId, string language = "en")
        {
            var version = GetLatestVersion(language);
            return version != null && version.Fields.ContainsKey(fieldId)
                ? version.Fields[fieldId]
                : null;
        }
        public Field? GetUnversionedField(Guid fieldId)
        {
            return SharedFields.FirstOrDefault(f => f.Id == fieldId);
        }
    }
}