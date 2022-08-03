using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Framework.CodeGeneration;
using ModelGenerator.Framework.CodeGeneration.FileTypes;
using ModelGenerator.IdClasses.Configuration;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ModelGenerator.IdClasses.CodeGeneration
{
    public class IdUsingGenerator : IUsingGenerator<DefaultFile>
    {
        private readonly IdSettings _settings;

        public IdUsingGenerator(IdSettings settings)
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