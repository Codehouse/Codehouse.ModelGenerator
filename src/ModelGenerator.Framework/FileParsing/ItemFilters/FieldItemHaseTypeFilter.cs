using System;
using System.Linq;
using ModelGenerator.Framework.Progress;

namespace ModelGenerator.Framework.FileParsing.ItemFilters
{
    public class FieldItemHaseTypeFilter : IItemFilter
    {
        private readonly Guid _fieldTypeFieldId;
        private readonly Guid _templateFieldTemplateId;

        public FieldItemHaseTypeFilter(FieldIds fieldIds, TemplateIds templateIds)
        {
            _fieldTypeFieldId = fieldIds.FieldType;
            _templateFieldTemplateId = templateIds.TemplateField;
        }

        public bool Accept(ScopedRagBuilder<string> tracker, Item item)
        {
            if (item.TemplateId != _templateFieldTemplateId)
            {
                return true;
            }

            if (item.SharedFields.Any(f => f.Id == _fieldTypeFieldId))
            {
                return true;
            }

            tracker.AddWarn($"Field item {item.Path} has no field type and will be ignored.");
            return false;
        }
    }
}