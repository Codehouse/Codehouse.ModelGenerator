using System.Collections.Generic;

namespace ModelGenerator.Fortis.Configuration
{
    public record FortisSettings
    {
        public record FieldTypeMappingSettings
        {
            public Dictionary<string, string[]> ConcreteFieldTypes { get; } = new Dictionary<string, string[]>();
            public string FallBackFieldType { get; init; }
            public Dictionary<string, string> FieldParameterMappings { get; } = new Dictionary<string, string>();
            public Dictionary<string, string[]> FieldValueMappings { get; } = new Dictionary<string, string[]>();
        }

        public record QuirkSettings
        {
            public bool LocalNamespaceForIds { get; init; }
            public bool PartialInterfaces { get; init; }
        }

        public FieldTypeMappingSettings FieldTypeMappings { get; } = new FieldTypeMappingSettings();
        public string[] NamespaceImports { get; init; }
        public QuirkSettings Quirks { get; } = new QuirkSettings();
    }
}