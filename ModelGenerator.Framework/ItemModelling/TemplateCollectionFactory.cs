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

        public TemplateCollectionFactory(IFieldIds fieldIds, ILogger<TemplateCollectionFactory> logger, ITemplateIds templateIds)
        {
            _fieldIds = fieldIds;
            _logger = logger;
            _templateIds = templateIds;
        }

        public TemplateCollection ConstructTemplates(IDatabase database)
        {
            var templateItems = database.GetItemsWhere(i => i.TemplateId == _templateIds.Template);
            var templateSets = templateItems
                               .GroupBy(i => database.GetItemSetForItem(i.Id))
                               .Select(g => CreateTemplateSet(database, g))
                               .Prepend(GetWellKnownTemplates())
                               .ToArray();

            var templates = templateSets
                            .SelectMany(s => s.Templates.Values)
                            .ToImmutableDictionary(t => t.Id);

            return new TemplateCollection(_templateIds)
            {
                TemplateSets = templateSets.ToImmutableDictionary(s => s.Id),
                Templates = templates
            };
        }

        protected string CreateNamespaceFromPath(string path, int trimFromStart, int trimFromEnd)
        {
            var pathParts = path
                            .Split('/', StringSplitOptions.RemoveEmptyEntries)
                            .Skip(trimFromStart)
                            .SkipLast(trimFromEnd);
            return string.Join('.', pathParts).Replace(" ", string.Empty);
        }

        protected virtual string ResolveLocalNamespace(IDatabase database, Item templateItem)
        {
            // TODO: This should be configurable (or work same as TDS)
            return CreateNamespaceFromPath(templateItem.Path, 4, 1);
        }

        private Template CreateTemplate(IDatabase database, Item templateItem)
        {
            var sections = database.GetChildren(templateItem.Id);
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
                LocalNamespace = ResolveLocalNamespace(database, templateItem),
                Path = templateItem.Path,
                SetId = templateItem.SetId
            };
        }

        private TemplateSet CreateTemplateSet(IDatabase database, IGrouping<ItemSet?, Item> grouping)
        {
            var items = grouping.ToArray();
            var groupName = grouping.Key;

            return new TemplateSet
            {
                Id = groupName.Id,
                Name = groupName.Name,
                ItemPath = groupName.ItemPath,
                ModelPath = groupName.ModelPath,
                Namespace = groupName.Namespace,
                References = groupName.References,
                Templates = items.Select(i => CreateTemplate(database, i))
                                 .ToImmutableDictionary(t => t.Id)
            };
        }

        private TemplateSet GetWellKnownTemplates()
        {
            // TODO: Make well-known templates configurable.
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