using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelGenerator.Framework.CodeGeneration.FileTypes;

namespace ModelGenerator.Framework.CodeGeneration
{
    public interface IUsingGenerator<TFile>
        where TFile : IFileType
    {
        IEnumerable<UsingDirectiveSyntax> GenerateUsings(TFile file);
    }
}