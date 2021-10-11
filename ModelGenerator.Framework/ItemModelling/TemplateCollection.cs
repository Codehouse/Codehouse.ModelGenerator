using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ModelGenerator.Framework.FileParsing;

namespace ModelGenerator.Framework.ItemModelling
{
    public class TemplateCollection
    {
        public IImmutableDictionary<Guid, Template> Templates { get; init; }
        public IImmutableDictionary<string, TemplateSet> TemplateSets { get; init; }

        private readonly IDictionary<Guid, Guid[]> _baseTemplateLookup = new Dictionary<Guid, Guid[]>();
        private readonly ITemplateIds _templateIds;

        public TemplateCollection(ITemplateIds templateIds)
        {
            _templateIds = templateIds;
        }

        public IEnumerable<TemplateField> GetAllFields(Guid templateId)
        {
            if (!Templates.ContainsKey(templateId))
            {
                return Enumerable.Empty<TemplateField>();
            }

            return GetAllBaseTemplates(templateId)
                   .Prepend(Templates[templateId])
                   .SelectMany(t => t.OwnFields)
                   .OrderBy(f => f.Name);
        }

        public IEnumerable<Template> GetAllBaseTemplates(Guid templateId)
        {
            return GetAllBaseTemplates(new HashSet<Guid>(), templateId)
                   .Where(t => Templates.ContainsKey(t))
                   .Select(t => Templates[t]);
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

            var baseTemplates = GetAllBaseTemplates(templateId);
            if (baseTemplates.Any(t => t.Id == _templateIds.RenderingParameters))
            {
                return TemplateTypes.RenderingParameter;
            }

            var template = Templates[templateId];
            return template.Name.StartsWith('_')
                ? TemplateTypes.Interface
                : TemplateTypes.Concrete;
        }

        private IEnumerable<Guid> GetAllBaseTemplates(HashSet<Guid> visitedTemplates, Guid templateId)
        {
            if (!Templates.ContainsKey(templateId))
            {
                return Enumerable.Empty<Guid>();
            }

            if (!visitedTemplates.Contains(templateId))
            {
                visitedTemplates.Add(templateId);
            }
            
            if (_baseTemplateLookup.ContainsKey(templateId))
            {
                return _baseTemplateLookup[templateId];
            }
            
            var template = Templates[templateId];
            var templates = template.BaseTemplateIds
                                    .SelectMany(t => GetAllBaseTemplates(visitedTemplates, t))
                                    .Union(template.BaseTemplateIds)
                                    .Distinct()
                                    .ToArray();
            _baseTemplateLookup.Add(templateId, templates);
            return templates;
        }
    }
}