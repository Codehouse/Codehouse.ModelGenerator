using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class TypeNameResolver
    {
        public string GetClassName(Template template)
        {
            return template.Name.TrimStart('_');
        }

        public string GetFullyQualifiedClassName(Template template, TemplateSet set)
        {
            return set.Name + "." + GetClassName(template);
        }

        public string GetFullyQualifiedInterfaceName(Template template, TemplateSet set)
        {
            return set.Name + ".I" + template.Name.TrimStart('_');
        }

        public string GetInterfaceName(Template template)
        {
            return "I" + template.Name.TrimStart('_');
        }

        public string GetRelativeInterfaceName(Template relativeTemplate, Template template, TemplateSet set)
        {
            return relativeTemplate.SetId == template.SetId
                ? GetInterfaceName(template)
                : GetFullyQualifiedInterfaceName(template, set);
        }
    }
}