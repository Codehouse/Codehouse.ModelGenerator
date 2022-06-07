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

                var namespaces = types
                    .GroupBy(x => x.Namespace)
                    .Select(g => GenerateTypeAndNamespace(g.Key, g))
                    .Cast<MemberDeclarationSyntax>()
                    .ToArray();

                return CompilationUnit()
                    .WithLeadingTrivia(Comment("// Generated"), EndOfLine(string.Empty))
                    .AddUsings(usings)
                    .AddMembers(namespaces);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not generate code for file {file.Model.FileName} in {file.Model.RootPath}");
                throw;
            }
        }
    }
}