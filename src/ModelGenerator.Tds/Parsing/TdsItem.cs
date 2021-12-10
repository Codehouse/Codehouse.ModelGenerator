using System;
using System.Collections.Immutable;
using ModelGenerator.Framework.FileParsing;

namespace ModelGenerator.Tds.Parsing
{
    public record TdsItem(
        Guid Id,
        string Name,
        Guid Parent,
        string Path,
        ImmutableList<Field> SharedFields,
        Guid TemplateId,
        string TemplateName,
        ImmutableList<LanguageVersion> Versions
    );
}