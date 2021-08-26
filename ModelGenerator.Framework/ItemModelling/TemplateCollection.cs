using System;
using System.Collections.Immutable;

namespace ModelGenerator.Framework.ItemModelling
{
    public class TemplateCollection
    {
        public IImmutableDictionary<string, TemplateSet> TemplateSets { get; init; }
        public IImmutableDictionary<Guid, Template> Templates { get; init; }
        public IImmutableDictionary<Guid, IImmutableList<Template>> TemplateHierarchy { get; init; }
    }
}