﻿using System;
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
    // TODO: Rename to StandardFileGenerator
    /// <summary>
    /// <para>
    /// Represents a "standard" file generator, and makes assumptions accordingly.
    /// </para>
    /// <para>
    /// This generator will collect usings for the specified <typeparamref name="TFile"/>,
    /// deduplicate and sort them according to standard rules.
    /// </para>
    /// <para>
    /// This generator will collect types for the specified <typeparamref name="TFile"/>,
    /// sort them according to the order defined in the configuration, and then group any
    /// adjacent types with the same namespace into a single namespace directive.
    /// </para>
    /// <para>
    /// If any custom file types do not need specific generation logic, they may register
    /// further instances of this for a fully working implementation.
    /// </para>
    /// </summary>
    /// <typeparam name="TFile">The file type</typeparam>
    public class DefaultFileGenerator<TFile> : FileGeneratorBase<TFile>
        where TFile : IFileType
    {
        private readonly ILogger<DefaultFileGenerator<TFile>> _logger;
        private readonly CodeGenerationSettings _settings;
        private readonly ITypeGenerator<TFile>[] _typeGenerators;
        private readonly IEnumerable<IUsingGenerator<TFile>> _usingGenerators;

        public DefaultFileGenerator(
            ILogger<DefaultFileGenerator<TFile>> logger,
            CodeGenerationSettings settings,
            Func<IEnumerable<IRewriter>> rewriterFactory,
            IEnumerable<ITypeGenerator<TFile>> typeGenerators,
            IEnumerable<IUsingGenerator<TFile>> usingGenerators)
            : base(logger, rewriterFactory)
        {
            _settings = settings;
            _logger = logger;
            _typeGenerators = OrderByConfig(typeGenerators).ToArray();
            _usingGenerators = usingGenerators;
        }

        protected override CompilationUnitSyntax? GenerateCode(TFile file)
        {
            try
            {
                var usings = _usingGenerators
                            .SelectMany(ug => ug.GenerateUsings(file))
                            .Distinct(new UsingSorter())
                            .OrderBy(s => s.Name.ToString(), new UsingSorter())
                            .ToArray();

                var namespaces = GenerateNamespaces(file);
                if (!namespaces.Any())
                {
                    _logger.LogWarning("No types were generated for the file {fileName} in {path}", file.Model.FileName, file.Model.RootPath);
                    return null;
                }

                // The file comment needs to be applied to the first namespace
                // TODO: Make this comment editable
                namespaces[0] = namespaces[0].WithLeadingTrivia(Comment("// Generated"), EndOfLine(string.Empty));

                return CompilationUnit()
                      .AddUsings(usings)
                      .AddMembers(namespaces.Cast<MemberDeclarationSyntax>().ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not generate code for file {fileName} in {rootPath}", file.Model.FileName, file.Model.RootPath);
                throw;
            }
        }

        private NamespaceDeclarationSyntax[] GenerateNamespaces(TFile file)
        {
            var types = _typeGenerators.Select(tg => tg.GenerateType(file))
                                       .WhereNotNull()
                                       .ToArray();
            if (!types.Any())
            {
                _logger.LogWarning("No types were generated for the file {fileName} in {path}", file.Model.FileName, file.Model.RootPath);
                return Array.Empty<NamespaceDeclarationSyntax>();
            }

            var typeGroups = GroupTypesByNamespace(types);
            return typeGroups.Where(typeGroup => typeGroup.Any())
                             .Select(typeGroup => GenerateTypeAndNamespace(typeGroup[0].Namespace, typeGroup))
                             .WhereNotNull()
                             .ToArray();
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
    }
}