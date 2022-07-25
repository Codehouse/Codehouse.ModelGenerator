using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.CodeGeneration.FileTypes;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration
{
    /// <summary>
    /// Base class that should be used for <see cref="IFileGenerator"/>
    /// implementations.
    /// <para>
    ///     Manages the actual creation of the file and writing any generated
    ///     code to it, but also ensures that any registered code rewriters
    ///     will be run on the code before it is written to disk. 
    /// </para>
    /// </summary>
    /// <typeparam name="TFile"></typeparam>
    public abstract class FileGeneratorBase<TFile> : IFileGenerator<TFile> where TFile : IFileType
    {
        private readonly ILogger<FileGeneratorBase<TFile>> _logger;
        private readonly Func<IEnumerable<IRewriter>> _rewriterFactory;

        protected FileGeneratorBase(
            ILogger<FileGeneratorBase<TFile>> logger,
            Func<IEnumerable<IRewriter>> rewriterFactory)
        {
            _logger = logger;
            _rewriterFactory = rewriterFactory;
        }

        public bool CanGenerate(IFileType file)
        {
            return file is TFile;
        }

        public FileInfo? GenerateFile(TFile file)
        {
            var syntax = GenerateCode(file);
            if (syntax == null)
            {
                return null;
            }

            return WriteCode(file.Model, syntax);
        }

        protected abstract CompilationUnitSyntax? GenerateCode(TFile file);

        protected NamespaceDeclarationSyntax? GenerateTypeAndNamespace(string @namespace, IEnumerable<NamespacedType> types)
        {
            var typeSyntax = types
                            .Select(t => t.Type)
                            .Cast<MemberDeclarationSyntax>()
                            .ToArray();

            return typeSyntax.Any()
                ? SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(@namespace))
                               .AddMembers(typeSyntax)
                : null;
        }

        private SyntaxNode FormatCode(SyntaxNode rootNode)
        {
            var rewriters = _rewriterFactory.Invoke();
            rootNode = rootNode.NormalizeWhitespace();

            foreach (var rewriter in rewriters)
            {
                try
                {
                    rootNode = rewriter.Visit(rootNode) ?? throw new InvalidOperationException("Rewriter returned null.");
                }
                catch (Exception e)
                {
                    var message = $"Failed to apply rewriter {rewriter.GetType().Name} to syntax tree.";
                    _logger.LogError(e, message);
                    throw new GenerationException(message, e);
                }
            }

            return rootNode;
        }

        FileInfo? IFileGenerator.GenerateFile(IFileType file)
        {
            if (file is TFile typedFile)
            {
                return GenerateFile(typedFile);
            }

            throw new NotSupportedException("The provided file type was not compatible with the generator.");
        }

        private FileInfo WriteCode(ModelFile modelFile, SyntaxNode syntax)
        {
            var filePath = Path.Combine(modelFile.RootPath, modelFile.FileName);
            if (!Directory.Exists(modelFile.RootPath))
            {
                Directory.CreateDirectory(modelFile.RootPath);
            }

            using var file = new StreamWriter(filePath, false);
            FormatCode(syntax)
               .WriteTo(file);

            file.Flush();
            return new FileInfo(filePath);
        }
    }
}