using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace ModelGenerator.Framework.FileParsing
{
    [DebuggerDisplay("Item: {Name} {Id}")]
    public record Item(
        ImmutableDictionary<HintTypes, string> Hints,
        Guid Id,
        string Name,
        Guid Parent,
        string Path,
        string RawFilePath,
        string SetId,
        ImmutableList<Field> SharedFields,
        Guid TemplateId,
        string TemplateName,
        ImmutableList<LanguageVersion> Versions
    )
    {
        public LanguageVersion? GetLatestVersion(string language = "en")
        {
            return Versions.Where(v => language.Equals(v.Language, StringComparison.OrdinalIgnoreCase))
                           .OrderBy(v => v.Number)
                           .LastOrDefault();
        }

        public Field? GetUnversionedField(Guid fieldId)
        {
            return SharedFields.FirstOrDefault(f => f.Id == fieldId);
        }

        public Field? GetVersionedField(Guid fieldId, string language = "en")
        {
            var version = GetLatestVersion(language);
            return version != null && version.Fields.ContainsKey(fieldId)
                ? version.Fields[fieldId]
                : null;
        }

        public bool HasHint(HintTypes hintType)
        {
            return Hints.ContainsKey(hintType);
        }
    }
}