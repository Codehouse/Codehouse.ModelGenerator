using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public interface IFortisFieldNameResolver : IFieldNameResolver
    {
        string GetFieldValueName(TemplateField field);
    }
}