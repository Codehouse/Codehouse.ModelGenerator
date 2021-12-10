using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.Configuration;
using ModelGenerator.Framework.ItemModelling;

namespace ModelGenerator.Framework.TypeConstruction
{
    public class TypeFactory : ITypeFactory
    {
        private readonly ILogger<TypeFactory> _log;
        private readonly Settings _settings;

        public TypeFactory(ILogger<TypeFactory> log, Settings settings)
        {
            _log = log;
            _settings = settings;
        }

        public IImmutableList<TypeSet> CreateTypeSets(TemplateCollection collection)
        {
            return collection.TemplateSets
                             .Values
                             .Select(s => CreateTypeSet(collection, s))
                             .WhereNotNull()
                             .ToImmutableList();
        }

        private ModelFile CreateFile(TemplateSet set, Template template, string rootPath)
        {
            var types = new[]
            {
                new ModelType(template.Name, template, set)
            };

            return new ModelFile(template.Name + ".Generated.cs", rootPath, types.ToImmutableList());
        }

        private TypeSet? CreateTypeSet(TemplateCollection templateCollection, TemplateSet templateSet)
        {
            if (string.IsNullOrEmpty(templateSet.ModelPath))
            {
                _log.LogWarning($"Template set {templateSet.Name} has no model path.");
                return null;
            }

            var rootPath = Path.Combine(templateSet.ModelPath, _settings.ModelFolder);
            var files = templateSet.Templates
                                   .Values
                                   .Select(t => CreateFile(templateSet, t, rootPath))
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
                References = referencedSets,
                RootPath = rootPath
            };
        }
    }
}