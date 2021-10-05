using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ModelGenerator.Framework.FileParsing;

namespace ModelGenerator.Framework.ItemModelling
{
    public class TemplateCollection
    {
        public IImmutableDictionary<Guid, IImmutableList<Template>> TemplateHierarchy { get; init; }
        public IImmutableDictionary<Guid, Template> Templates { get; init; }
        public IImmutableDictionary<string, TemplateSet> TemplateSets { get; init; }

        private readonly IDictionary<Guid, IImmutableList<TemplateField>> _allFieldsLookup = new Dictionary<Guid, IImmutableList<TemplateField>>();
        private readonly ITemplateIds _templateIds;

        public TemplateCollection(ITemplateIds templateIds)
        {
            _templateIds = templateIds;
        }

        public IImmutableList<TemplateField> GetAllFields(Guid templateId)
        {
            return GetAllFields(new HashSet<Guid>(), templateId);
        }
        
        public bool IsRenderingParameters(Guid templateId)
        {
            return GetTemplateType(templateId) == TemplateTypes.RenderingParameter;
        }

        public TemplateTypes GetTemplateType(Guid templateId)
        {
            if (!Templates.ContainsKey(templateId))
            {
                throw new ArgumentException($"Unknown template ID {templateId}", nameof(templateId));
            }
            
            // TODO: Check for indirect RP template inheritance
            if (templateId == _templateIds.RenderingParameters)
            {
                return TemplateTypes.RenderingParameter;
            }

            var baseTemplates = TemplateHierarchy[templateId];
            if (baseTemplates != null && baseTemplates.Any(t => t.Id == _templateIds.RenderingParameters))
            {
                return TemplateTypes.RenderingParameter;
            }

            var template = Templates[templateId];
            return template.Name.StartsWith('_')
                ? TemplateTypes.Interface
                : TemplateTypes.Concrete;
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