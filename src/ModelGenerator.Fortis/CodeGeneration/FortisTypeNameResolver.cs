using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.Configuration;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FortisTypeNameResolver : TypeNameResolver, IFortisTypeNameResolver
    {
        public FortisTypeNameResolver(Settings settings) : base(settings)
        {
        }

        public string GetClassName(Template template)
        {
            return GetTypeName(template) + "Item";
        }
    }
}