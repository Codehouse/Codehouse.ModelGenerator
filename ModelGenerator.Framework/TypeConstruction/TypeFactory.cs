using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Framework.TypeConstruction
{
    public class TypeFactory : ITypeFactory
    {
        private readonly ILogger<TypeFactory> _log;

        public TypeFactory(ILogger<TypeFactory> log)
        {
            _log = log;
        }
        
        public IImmutableList<TypeSet> CreateTypeSets(TemplateCollection collection)
        {
            return collection.TemplateSets
                             .Values
                             .Select(CreateTypeSet)
                             .Where(s => s != null)
                             .ToImmutableList();
        }

        private TypeSet? CreateTypeSet(TemplateSet templateSet)
        {
            if (string.IsNullOrEmpty(templateSet.ModelPath))
            {
                _log.LogWarning($"Template set {templateSet.Name} has no model path.");
                return null;
            }
            
            var files = templateSet.Templates
                           .Values
                           .Select(t => CreateFile(templateSet, t))
                           .ToImmutableList();

            return new TypeSet
            {
                Name = templateSet.Name,
                Files = files
            };
        }

        private ModelFile CreateFile(TemplateSet set, Template template)
        {
            var types = new ModelType[]
            {
                new ModelInterface {Template = template, Name = template.Name, Fields = template.OwnFields},
                new ModelClass {Template = template, Name = template.Name, Fields = template.OwnFields},
                new ModelIdType {Templates = {template}}
            };

            // TODO: Work out Imports
            return new ModelFile
            {
                Namespace = set.Name + ".Models",
                Path = Path.Combine(set.ModelPath, "Models"),
                FileName = template.Name + ".cs",
                Types = types.ToImmutableList()
            };
        }
    }
}