using System.Collections.Immutable;
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
                             .Select(s => CreateTypeSet(collection, s))
                             .Where(s => s != null)
                             .ToImmutableList();
        }

        private ModelFile CreateFile(TemplateSet set, Template template)
        {
            var types = new []
            {
                new ModelType(template.Name, template, set)
            };

            return new ModelFile
            {
                RootPath = set.ModelPath,
                FileName = template.Name + ".Generated.cs",
                Types = types.ToImmutableList()
            };
        }

        private TypeSet? CreateTypeSet(TemplateCollection templateCollection, TemplateSet templateSet)
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

            var referencedSets = templateCollection.TemplateSets
                                                   .Values
                                                   .Where(s => templateSet.References.Contains(s.Id))
                                                   .ToImmutableArray();
            
            return new TypeSet
            {
                Name = templateSet.Name,
                Files = files,
                Namespace = templateSet.Namespace,
                References = referencedSets
            };
        }
    }
}