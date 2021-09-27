using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ModelGenerator.Framework.ItemModelling
{
    public class TemplateCollection
    {
        public IImmutableDictionary<string, TemplateSet> TemplateSets { get; init; }
        public IImmutableDictionary<Guid, Template> Templates { get; init; }
        public IImmutableDictionary<Guid, IImmutableList<Template>> TemplateHierarchy { get; init; }

        private readonly IDictionary<Guid, IImmutableList<TemplateField>> _allFieldsLookup = new Dictionary<Guid, IImmutableList<TemplateField>>();
        
        public IImmutableList<TemplateField> GetAllFields(Guid templateId)
        {
            return GetAllFields(new HashSet<Guid>(), templateId);
        }

        private IImmutableList<TemplateField> GetAllFields(HashSet<Guid> visitedTemplates, Guid templateId)
        {
            if (!Templates.ContainsKey(templateId))
            {
                return ImmutableList<TemplateField>.Empty;
            }

            if (visitedTemplates.Contains(templateId))
            {
                // Potential circular reference
                return ImmutableList<TemplateField>.Empty;
            }

            visitedTemplates.Add(templateId);
            if (_allFieldsLookup.ContainsKey(templateId))
            {
                return _allFieldsLookup[templateId];
            }
            
            var template = Templates[templateId];
            var fields = template.BaseTemplateIds
                                 .SelectMany(t => GetAllFields(visitedTemplates, t))
                                 .Union(template.OwnFields)
                                 .GroupBy(f => f.Id)
                                 .Select(g => g.First())
                                 .OrderBy(f => f.Name)
                                 .ToImmutableList();
            
            _allFieldsLookup.Add(templateId, fields);
            return fields;
        }
    }
}