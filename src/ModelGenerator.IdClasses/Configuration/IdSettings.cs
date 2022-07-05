using System;
using System.Collections.Generic;

namespace ModelGenerator.IdClasses.Configuration
{
    public record IdSettings
    {
        public record QuirkSettings
        {
            /// <summary>
            ///     Toggle whether or not to use local namespaces for field and template ID classes.
            ///     <para>If true, field and template ID classes for a template will use that template's local namespace.</para>
            ///     <para>If false, field and template ID classes for a template will use the template set's namespace.</para>
            /// </summary>
            public bool LocalNamespaceForIds { get; init; }
        }

        public string FieldIdsTypeName { get; init; } = string.Empty;
        public QuirkSettings Quirks { get; } = new();
        public string TemplateIdsTypeName { get; init; } = string.Empty;
    }
}