using System;
using System.Linq;

namespace ModelGenerator.Framework.FileParsing
{
    public class TemplateFilter : IItemFilter
    {
        private readonly Guid[] _validIds;

        public TemplateFilter(ITemplateIds templateIds)
        {
            _validIds = new[]
            {
                templateIds.Template,
                templateIds.TemplateSection,
                templateIds.TemplateField
            };
        }

        public bool Accept(Item item)
        {
            return _validIds.Contains(item.TemplateId);
        }
    }
}