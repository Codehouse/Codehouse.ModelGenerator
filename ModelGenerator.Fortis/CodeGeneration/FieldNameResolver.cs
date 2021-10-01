using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FieldNameResolver
    {
        public string GetFieldName(TemplateField field)
        {
            return field.Name.Replace(" ", "");
        }

        public string GetFieldValueName(TemplateField field)
        {
            return GetFieldName(field) + "Value";
        }
    }
}