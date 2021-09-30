using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public interface IFieldTypeResolver
    {
        string GetFieldInterfaceType(TemplateField field);
        string GetFieldConcreteType(TemplateField field);
        string GetFieldValueType(TemplateField field);
    }
}