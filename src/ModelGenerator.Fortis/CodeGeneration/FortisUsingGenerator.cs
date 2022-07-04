using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Fortis.Configuration;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.CodeGeneration.FileTypes;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ModelGenerator.Fortis.CodeGeneration
{
    public class FortisUsingGenerator : IUsingGenerator<DefaultFile>
    {
        private readonly FortisSettings _settings;

        public FortisUsingGenerator(FortisSettings settings)
        {
            _settings = settings;
        }

        public IEnumerable<UsingDirectiveSyntax> GenerateUsings(DefaultFile file)
        {
            // TODO: Add support for namespace and type aliasing
            return _settings.NamespaceImports
                            .Select(ns => UsingDirective(ParseName(ns)))
                            .ToArray();
        }
    }
}