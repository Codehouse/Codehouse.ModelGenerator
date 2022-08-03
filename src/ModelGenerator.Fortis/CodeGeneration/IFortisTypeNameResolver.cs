using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public interface IFortisTypeNameResolver : ITypeNameResolver
    {
        string GetClassName(Template template);
    }
}