using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.FileParsing;

namespace ModelGenerator.Framework.ItemModelling
{
    public class TemplateCollectionFactory : ITemplateCollectionFactory
    {
        private readonly IFieldIds _fieldIds;
        private readonly ILogger<TemplateCollectionFactory> _logger;
        private readonly ITemplateIds _templateIds;

        public TemplateCollectionFactory(IFieldIds fieldIds, ILogger<TemplateCollectionFactory> _logger, ITemplateIds templateIds)
        {
            _fieldIds = fieldIds;
            this._logger = _logger;
            _templateIds = templateIds;
        }

        public TemplateCollection ConstructTemplates(IDatabase database)
        {
            var templateItems = database.GetItemsWhere(i => i.TemplateId == _templateIds.Template);
            var templateSets = templateItems
                               .GroupBy(i => database.GetItemSetForItem(i.Id))
                               .Select(g => new TemplateSet
                               {
                                   Id = g.Key.Id,
                                   Name = g.Key.Name,
                                   ItemPath = g.Key.ItemPath,
                                   ModelPath = g.Key.ModelPath,
                                   References = g.Key.References,
                                   Templates = g.Select(i => CreateTemplate(database, i))
                                                .ToImmutableDictionary(t => t.Id)
                               })
                               .Prepend(GetWellKnownTemplates())
                               .ToArray();

            var templates = templateSets
                            .SelectMany(s => s.Templates.Values)
                            .ToImmutableDictionary(t => t.Id);
            var hierarchy = templateSets
                            .SelectMany(s => s.Templates.Values)
                            .Select(t => KeyValuePair.Create(t.Id, GetBaseTemplates(t, templates)))
                            .ToImmutableDictionary();

            return new TemplateCollection
            {
                TemplateSets = templateSets.ToImmutableDictionary(s => s.Id),
                Templates = templates,
                TemplateHierarchy = hierarchy
            };
        }

        private Template CreateTemplate(IDatabase database, Item templateItem)
        {
            var sections = database
                .GetChildren(templateItem.Id);
            var fields = sections
                         .SelectMany(section => database.GetChildren(section.Id), (section, fieldItem) => new TemplateField
                         {
                             Id = fieldItem.Id,
                             Name = fieldItem.Name,
                             Item = fieldItem,
                             DisplayName = fieldItem.GetVersionedField(_fieldIds.DisplayName)?.Value,
                             SectionName = section.Name,
                             FieldType = fieldItem.GetUnversionedField(_fieldIds.FieldType)?.Value,
                             TemplateId = templateItem.Id
                         })
                         .OrderBy(f => f.Name)
                         .ToImmutableList();

            var baseTemplates = templateItem.SharedFields
                                            .SingleOrDefault(f => f.Id == _fieldIds.BaseTemplates)
                                            .GetMultiReferenceValue();

            return new Template
            {
                Id = templateItem.Id,
                Name = templateItem.Name,
                Item = templateItem,
                DisplayName = templateItem.GetVersionedField(_fieldIds.DisplayName)?.Value,
                OwnFields = fields,
                BaseTemplateIds = baseTemplates ?? new Guid[0],
                Path = templateItem.Path,
                SetId = templateItem.SetId,
                TemplateType = GetTemplateType(templateItem, baseTemplates)
            };
        }

        private IImmutableList<Template> GetBaseTemplates(Template t, ImmutableDictionary<Guid, Template> templates)
        {
            return t.BaseTemplateIds.Select(id =>
                    {
                        if (templates.ContainsKey(id))
                        {
                            return templates[id];
                        }

                        _logger.LogWarning($"Unknown base template {id} on template {t.Name} ({t.Id})");
                        return null;
                    })
                    .Where(t => t != null)
                    .ToImmutableList();
        }

        private TemplateTypes GetTemplateType(Item templateItem, Guid[]? baseTemplates)
        {
            // TODO: Check for indirect RP template inheritance
            if (baseTemplates != null && baseTemplates.Contains(_templateIds.RenderingParameters))
            {
                return TemplateTypes.RenderingParameter;
            }

            return templateItem.Name.StartsWith('_')
                ? TemplateTypes.Interface
                : TemplateTypes.Concrete;
        }

        private TemplateSet GetWellKnownTemplates()
        {
            var folderTemplateId = Guid.Parse("{A87A00B1-E6DB-45AB-8B54-636FEC3B5523}");
            var standardTemplateId = Guid.Parse("{1930BBEB-7805-471A-A3BE-4858AC7CF696}");
            var templates = new Dictionary<Guid, Template>
            {
                {
                    standardTemplateId,
                    new Template
                    {
                        Id = standardTemplateId,
                        Name = "Standard Template",
                        DisplayName = "Standard Template",
                        OwnFields = ImmutableList<TemplateField>.Empty,
                        BaseTemplateIds = new Guid[0]
                    }
                },
                {
                    folderTemplateId,
                    new Template
                    {
                        Id = folderTemplateId,
                        Name = "Folder",
                        DisplayName = "Folder",
                        OwnFields = ImmutableList<TemplateField>.Empty,
                        BaseTemplateIds = new Guid[0]
                    }
                }
            };

            return new TemplateSet
            {
                Id = string.Empty,
                Name = "Well Known",
                Templates = templates.ToImmutableDictionary()
            };
        }
    }
}