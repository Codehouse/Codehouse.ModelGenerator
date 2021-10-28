using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.Configuration;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Tds.ItemModelling
{
    public class TdsTemplateCollectionFactory : TemplateCollectionFactory
    {
        private readonly Dictionary<string, int> _commonFolderDepths = new Dictionary<string, int>();
        private readonly Settings _settings;
        private readonly ITemplateIds _templateIds;

        public TdsTemplateCollectionFactory(IFieldIds fieldIds, ILogger<TemplateCollectionFactory> logger, Settings settings, ITemplateIds templateIds)
            : base(fieldIds, logger, templateIds)
        {
            _settings = settings;
            _templateIds = templateIds;
        }

        protected override string ResolveLocalNamespace(IDatabase database, Item templateItem)
        {
            // TODO the original TDS functionality wasn't reverse-engineered successfully.  This is as far as I got.
            var namespaces = BuildLocalNamespaceStack(database, new List<string>(), templateItem.Parent);
            if (namespaces.Count == 0 || namespaces[0] == "sitecore")
            {
                if (!_commonFolderDepths.ContainsKey(templateItem.SetId))
                {
                    _commonFolderDepths[templateItem.SetId] = CountCommonFolderDepth(database.GetItemSetForItem(templateItem.Id).Items.Values);
                }

                return CreateNamespaceFromPath(templateItem.Path, _commonFolderDepths[templateItem.SetId], 1);
            }

            return string.Join(".", namespaces).Replace(" ", "");
        }

        private List<string> BuildLocalNamespaceStack(IDatabase database, List<string> values, Guid itemId)
        {
            if (itemId == Guid.Empty)
            {
                return values;
            }

            var item = database.GetItem(itemId);
            if (item == null)
            {
                return values;
            }

            if (item.HasHint(HintTypes.Namespace))
            {
                var namespaceHint = item.Hints[HintTypes.Namespace];
                if (namespaceHint != _settings.ModelNamespace)
                {
                    if (namespaceHint.StartsWith(_settings.ModelNamespace))
                    {
                        namespaceHint = namespaceHint.Substring(_settings.ModelNamespace.Length).TrimStart('.');
                    }
                    
                    values.Insert(0, namespaceHint);
                }

                return values;
            }
            
            values.Insert(0, item.Name);
            return BuildLocalNamespaceStack(database, values, item.Parent);
        }

        private int CountCommonFolderDepth(IEnumerable<Item> items)
        {
            var pathFragmentsCollection = items
                                          .Where(i => i.TemplateId == _templateIds.Template)
                                          .Select(i => i.Path)
                                          .Select(p => p.Split("/", StringSplitOptions.RemoveEmptyEntries))
                                          .OrderByDescending(p => p.Length)
                                          .ToArray();

            for (int i = 0; i < pathFragmentsCollection[0].Length; i++)
            {
                var element = pathFragmentsCollection[0][i];
                for (int j = 0; j < pathFragmentsCollection.Length; j++)
                {
                    if (pathFragmentsCollection[j][i] != element)
                    {
                        return i;
                    }
                }
            }

            return pathFragmentsCollection[0].Length - 1;
        }
    }
}