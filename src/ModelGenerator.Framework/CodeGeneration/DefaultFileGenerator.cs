using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.CodeGeneration.FileTypes;
using ModelGenerator.Framework.Configuration;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ModelGenerator.Framework.CodeGeneration
{
    public class DefaultFileGenerator<TFile> : FileGeneratorBase<TFile>
        where TFile : IFileType
    {
        private ILogger<DefaultFileGenerator<TFile>> _logger;
        private readonly CodeGenerationSettings _settings;
        private readonly ITypeGenerator<TFile>[] _typeGenerators;
        private readonly IEnumerable<IUsingGenerator<TFile>> _usingGenerators;

        public DefaultFileGenerator(
            ILogger<DefaultFileGenerator<TFile>> logger,
            CodeGenerationSettings settings,
            Func<IEnumerable<IRewriter>> rewriterFactory,
            IEnumerable<ITypeGenerator<TFile>> typeGenerators,
            IEnumerable<IUsingGenerator<TFile>> usingGenerators)
        :base(logger, rewriterFactory)
        {
            _settings = settings;
            _logger = logger;
            _typeGenerators = OrderByConfig(typeGenerators).ToArray();
            _usingGenerators = usingGenerators;
        }

        private IEnumerable<ITypeGenerator<TFile>> OrderByConfig(IEnumerable<ITypeGenerator<TFile>> typeGenerators)
        {
            var set = typeGenerators.ToDictionary(t => t.Tag);
            foreach (var tag in _settings.TypeGenerationOrder)
            {
                if (set.ContainsKey(tag))
                {
                    yield return set[tag];
                    set.Remove(tag);
                }
            }

            if (set.Any())
            {
                _logger.LogWarning("There were unordered type generators that were omitted: {generators}", string.Join(" ", set.Keys));
            }
        }

        protected override CompilationUnitSyntax? GenerateCode(TFile file)
        {
            try
            {
                // TODO: Sort and deduplicate usings?
                var usings = _usingGenerators
                    .SelectMany(ug => ug.GenerateUsings(file))
                    .ToArray();

                var types = _typeGenerators.Select(tg => tg.GenerateType(file))
                    .WhereNotNull()
                    .ToArray();
                if (!types.Any())
                {
                    _logger.LogWarning("No types were generated for the file {fileName} in {path}", file.Model.FileName, file.Model.RootPath);
                    return null;
                }

                _logger.LogDebug("Generated type order for {fileName}: {types}", file.Model.FileName, string.Join(' ', types.Select(t => t.TypeName)));


                var unit = CompilationUnit()
                           .AddUsings(usings);
                
                var namespaces = GroupTypesByNamespace(types);
                var isFirst = true;
                
                
                _logger.LogDebug("Grouped type order for {fileName}: {types}", file.Model.FileName, string.Join(',', namespaces.Select(g => "[" + string.Join(' ', g.Select(t => t.TypeName)) + "]" )));
                
                foreach (var typeGroup in namespaces)
                {
                    var @namespace = GenerateTypeAndNamespace(typeGroup.First().Namespace, typeGroup);
                    if (@namespace == null)
                    {
                        continue;
                    }

                    if (isFirst)
                    {
                        isFirst = false;
                        @namespace = @namespace.WithLeadingTrivia(Comment("// Generated"), EndOfLine(string.Empty));
                    }

                    unit = unit.AddMembers(@namespace);
                }

                return unit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not generate code for file {fileName} in {rootPath}", file.Model.FileName, file.Model.RootPath);
                throw;
            }
        }

        private IEnumerable<NamespacedType[]> GroupTypesByNamespace(NamespacedType[] types)
        {
            // Using a group by disturbs the configured order.  Also note that depending on order, the same namespace may appear twice.
            while (types.Any())
            {
                var currentNamespace = types[0].Namespace;
                var typeGroup = types.TakeWhile(t => t.Namespace == currentNamespace).ToArray();
                types = types.Skip(typeGroup.Length).ToArray();
                yield return typeGroup;
            }
        }
    }
}