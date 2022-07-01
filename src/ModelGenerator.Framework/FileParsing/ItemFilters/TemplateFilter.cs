using System;
using System.Linq;
using ModelGenerator.Framework.Progress;

namespace ModelGenerator.Framework.FileParsing.ItemFilters
{
    public class TemplateFilter : IItemFilter
    {
        private readonly Guid[] _validIds;

        public TemplateFilter(TemplateIds templateIds)
        {
            _validIds = new[]
            {
                templateIds.Template,
                templateIds.TemplateSection,
                templateIds.TemplateField
            };
        }

        public bool Accept(ScopedRagBuilder<string> tracker, Item item)
        {
            return _validIds.Contains(item.TemplateId);
        }
    }
}