using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Framework.CodeGeneration
{
    public interface IFieldNameResolver
    {
        string GetFieldName(TemplateField field);
    }
}