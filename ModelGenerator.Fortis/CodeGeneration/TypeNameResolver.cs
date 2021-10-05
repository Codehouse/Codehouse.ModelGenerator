using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class TypeNameResolver
    {
        public string GetClassName(Template template)
        {
            return template.Name.TrimStart('_').Replace(" ", string.Empty);
        }

        public string GetFullyQualifiedInterfaceName(Template template, TemplateSet set)
        {
            return set.Namespace + "." + GetInterfaceName(template);
        }

        public string GetInterfaceName(Template template)
        {
            return "I" + GetClassName(template);
        }

        public string GetRelativeInterfaceName(Template relativeTemplate, Template template, TemplateSet set)
        {
            return relativeTemplate.SetId == template.SetId
                ? GetInterfaceName(template)
                : GetFullyQualifiedInterfaceName(template, set);
        }
    }
}