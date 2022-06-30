using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Framework.CodeGeneration
{
    public class FieldNameResolver : IFieldNameResolver
    {
        public string GetFieldName(TemplateField field)
        {
            return field.Name.Replace(" ", "");
        }
    }
}