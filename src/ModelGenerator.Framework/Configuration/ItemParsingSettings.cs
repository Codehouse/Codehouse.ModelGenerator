using System;

namespace ModelGenerator.Framework.Configuration
{
    public class ItemParsingSettings
    {
        public string[] IncludedFields { get; init; } = Array.Empty<string>();
        public string[] InternedFieldValues { get; init; } = Array.Empty<string>();
    }
}