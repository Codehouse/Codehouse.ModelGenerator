using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Framework.TypeConstruction;

namespace ModelGenerator.Framework.CodeGeneration
{
    public class CodeGenerator : ICodeGenerator
    {
        private readonly IFileGenerator _fileGenerator;
        private readonly ILogger<CodeGenerator> _logger;
        private readonly Func<IEnumerable<IRewriter>> _rewriterFactory;

        public CodeGenerator(IFileGenerator fileGenerator, ILogger<CodeGenerator> logger, Func<IEnumerable<IRewriter>> rewriterFactory)
        {
            _fileGenerator = fileGenerator;
            _logger = logger;
            _rewriterFactory = rewriterFactory;
        }

        public FileInfo? GenerateFile(ScopedRagBuilder<string> ragBuilder, GenerationContext context, ModelFile modelFile)
        {
            var syntax = GenerateCode(ragBuilder, context, modelFile);
            if (syntax == null)
            {
                return null;
            }

            return WriteCode(modelFile, syntax);
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

        private CompilationUnitSyntax? GenerateCode(ScopedRagBuilder<string> ragBuilder, GenerationContext context, ModelFile modelFile)
        {
            try
            {
                return _fileGenerator.GenerateCode(ragBuilder, context, modelFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not generate code for file {modelFile.FileName} in {modelFile.RootPath}");
                throw;
            }
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