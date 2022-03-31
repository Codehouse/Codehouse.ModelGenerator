using System;
using System.Collections.Generic;

namespace ModelGenerator.Fortis.Configuration
{
    public record FortisSettings
    {
        public record FieldTypeMappingSettings
        {
            public Dictionary<string, string> ConcreteFieldTypes { get; } = new(StringComparer.OrdinalIgnoreCase);
            public string FallBackFieldType { get; init; } = string.Empty;
            public Dictionary<string, string> FieldParameterMappings { get; } = new(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, string> FieldValueMappings { get; } = new(StringComparer.OrdinalIgnoreCase);
        }

        public record QuirkSettings
        {
            /// <summary>
            /// Toggle whether or not to cast calls to <c>GetField<c> for rendering parameters.
            /// <para>Some projects have overridden the rendering parameters wrapper to add an equivalent
            /// <c>GetField&lt;T&gt</c> method. Others have not.</para>
            /// <para>If true, calls a non-generic <c>GetField</c> method and casts the result.</para>
            /// <para>If false, calls a generic <c>GetField</c> method.</para>
            /// </summary>
            public bool CastRenderingParameterFields { get; set; }
            
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

        public record TypeNameSettings
        {
            public string ItemWrapper { get; init; } = string.Empty;
            public string ItemWrapperInterface { get; init; } = string.Empty;
            public string RenderingParameter { get; init; } = string.Empty;
            public string RenderingParameterInterface { get; init; } = string.Empty;
            public string SitecoreItem { get; init; } = string.Empty;
            public string SpawnProvider { get; init; } = string.Empty;
        }

        public FieldTypeMappingSettings FieldTypeMappings { get; } = new();
        public string[] NamespaceImports { get; init; } = Array.Empty<string>();
        public QuirkSettings Quirks { get; } = new();

        public TypeNameSettings TypeNames { get; } = new();
    }
}