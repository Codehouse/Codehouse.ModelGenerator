using System.Collections.Generic;

namespace ModelGenerator.Fortis.Configuration
{
    public record FortisSettings
    {
        public record FieldTypeMappingSettings
        {
            public Dictionary<string, string[]> ConcreteFieldTypes { get; init; }
            public string FallBackFieldType { get; init; }
            public Dictionary<string, string> FieldParameterMappings { get; init; }
            public Dictionary<string, string[]> FieldValueMappings { get; init; }
        }
        public FieldTypeMappingSettings FieldTypeMappings { get; init; }
        public string[] NamespaceImports { get; init; }
    }
}