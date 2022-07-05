using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration
{
    public interface ITypeNameResolver
    {
        string GetFullyQualifiedInterfaceName(Template template, TemplateSet set);

        string GetInterfaceName(Template template);

        string GetNamespace(TemplateSet set, Template template);

        string GetNamespace(TypeSet set, Template template);

        string GetRelativeInterfaceName(Template relativeTemplate, Template template, TemplateSet set);

        string GetTypeName(Template template);
    }
}