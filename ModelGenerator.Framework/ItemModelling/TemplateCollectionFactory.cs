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
        private readonly FieldIds _fieldIds;
        private readonly ILogger<TemplateCollectionFactory> _logger;
        private readonly TemplateIds _templateIds;

        public TemplateCollectionFactory(FieldIds fieldIds, ILogger<TemplateCollectionFactory> logger, TemplateIds templateIds)
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

            var collection = new TemplateCollection(_templateIds, templates, templateSets);

            // Early initialisation of all these caches removes need for locking during parallel generation.
            foreach (var template in templates.Keys)
            {
                collection.GetAllBaseTemplates(template);
                collection.GetAllFields(template);
            }

            return collection;
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
            var sections = database.GetChildren(templateItem.Id).Where(f => f.TemplateId == _templateIds.TemplateSection);
            var fields = sections
                         .SelectMany(section => database.GetChildren(section.Id), (section, fieldItem) => new TemplateField
                         (
                             fieldItem.GetVersionedField(_fieldIds.DisplayName)?.Value ?? fieldItem.Name,
                             fieldItem.GetUnversionedField(_fieldIds.FieldType)?.Value ?? throw new FrameworkException($"Field {fieldItem.Path} has no field type."),
                             fieldItem.Id,
                             fieldItem,
                             fieldItem.Name,
                             section.Name,
                             templateItem.Id
                         ))
                         .OrderBy(f => f.Name)
                         .ToImmutableList();

            var baseTemplates = templateItem.SharedFields
                                            .SingleOrDefault(f => f.Id == _fieldIds.BaseTemplates)
                                            .GetMultiReferenceValue();

            return new Template(
                baseTemplates,
                templateItem.GetVersionedField(_fieldIds.DisplayName)?.Value ?? templateItem.Name,
                templateItem.Id,
                templateItem,
                ResolveLocalNamespace(database, templateItem),
                templateItem.Name,
                fields,
                templateItem.Path,
                templateItem.SetId
            );
        }

        private TemplateSet CreateTemplateSet(IDatabase database, IGrouping<ItemSet?, Item> grouping)
        {
            var items = grouping.ToArray();
            var groupName = grouping.Key;
            if (groupName == null)
            {
                throw new FrameworkException("Template set was null during template collection creation.");
            }

            return new TemplateSet(
                groupName.Id,
                groupName.ItemPath,
                groupName.ModelPath,
                groupName.Name,
                groupName.Namespace,
                groupName.References,
                items.Select(i => CreateTemplate(database, i))
                     .ToImmutableDictionary(t => t.Id)
            );
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
                    new Template(
                        new Guid[0],
                        "Standard Template",
                        standardTemplateId,
                        null,
                        string.Empty,
                        "Standard Template",
                        ImmutableList<TemplateField>.Empty,
                        string.Empty,
                        string.Empty
                    )
                },
                {
                    folderTemplateId,
                    new Template(
                        new Guid[0],
                        "Folder",
                        folderTemplateId,
                        null,
                        string.Empty,
                        "Folder",
                        ImmutableList<TemplateField>.Empty,
                        string.Empty,
                        string.Empty
                    )
                }
            };

            return new TemplateSet(
                string.Empty,
                string.Empty,
                string.Empty,
                "Well Known",
                string.Empty,
                ImmutableArray<string>.Empty,
                templates.ToImmutableDictionary()
            );
        }
    }
}