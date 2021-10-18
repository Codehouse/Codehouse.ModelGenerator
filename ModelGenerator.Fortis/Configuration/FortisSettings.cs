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
            /// <summary>
            /// Toggle whether or not interfaces for the full inheritance tree are added to template classes.
            /// <para>If true, adds all base templates</para>
            /// <para>If false, adds only direct base templates</para>
            /// </summary>
            public bool FullInterfaceList { get; init; }
            
            /// <summary>
            /// Toggle whether or not to use local namespaces for field and template ID classes.
            /// <para>If true, field and template ID classes for a template will use that template's local namespace.</para>
            /// <para>If false, field and template ID classes for a template will use the template set's namespace.</para>
            /// </summary>
            public bool LocalNamespaceForIds { get; init; }
            
            /// <summary>
            /// Toggle whether or not to mark template interfaces as partial.
            /// <para>If true, interfaces will be marked partial.</para>
            /// <para>If false, interfaces will not be marked partial.</para>
            /// </summary>
            public bool PartialInterfaces { get; init; }
        }

        public FieldTypeMappingSettings FieldTypeMappings { get; } = new FieldTypeMappingSettings();
        public string[] NamespaceImports { get; init; }
        public QuirkSettings Quirks { get; } = new QuirkSettings();
    }
}