using System.Linq;
using ModelGenerator.Fortis.Configuration;
using ModelGenerator.Framework.ItemModelling;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class TypeNameResolver
    {
        private readonly FortisSettings _fortisSettings;

        public TypeNameResolver(FortisSettings fortisSettings)
        {
            _fortisSettings = fortisSettings;
        }
        
        public string GetClassName(Template template)
        {
            return GetTypeName(template) + "Item";
        }

        public string GetFullyQualifiedInterfaceName(Template template, TemplateSet set)
        {
            // TODO: Apply model namespace setting here instead of in model construction.
            return JoinNamespace(set.Namespace, template.LocalNamespace, GetInterfaceName(template));
        }

        public string GetInterfaceName(Template template)
        {
            return "I" + GetTypeName(template);
        }

        public string GetNamespace(TemplateSet set, Template template)
        {
            return GetNamespace(set.Namespace, template);
        }

        public string GetNamespace(TypeSet set, Template template)
        {
            return GetNamespace(set.Namespace, template);
        }

        public string GetRelativeInterfaceName(Template relativeTemplate, Template template, TemplateSet set)
        {
            return relativeTemplate.SetId == template.SetId
                ? JoinNamespace(_fortisSettings.ModelNamespace, template.LocalNamespace, GetInterfaceName(template))
                : GetFullyQualifiedInterfaceName(template, set);
        }

        public string GetTypeName(Template template)
        {
            return template.Name.TrimStart('_').Replace(" ", string.Empty);
        }

        private string GetNamespace(string setNamespace, Template template)
        {
            return JoinNamespace(setNamespace, template.LocalNamespace);
        }

        private string JoinNamespace(params string[] parts)
        {
            return string.Join(".", parts.Where(s => !string.IsNullOrEmpty(s)));
        }
    }
}