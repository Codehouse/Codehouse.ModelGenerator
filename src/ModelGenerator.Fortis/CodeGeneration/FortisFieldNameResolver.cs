using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FortisFieldNameResolver : FieldNameResolver, IFortisFieldNameResolver
    {
        public string GetFieldValueName(TemplateField field)
        {
            return GetFieldName(field) + "Value";
        }
    }
}