using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Framework.FileScanning;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Tds.Parsing;

namespace ModelGenerator.Tds
{
    internal class TdsFileParser : IFileParser
    {
        private readonly IItemFilter[] _itemFilters;
        private readonly ILogger<TdsFileParser> _logger;
        private readonly ITdsItemParser _parser;
        private readonly ITdsTokenizer _tokenizer;

        public TdsFileParser(
            ILogger<TdsFileParser> logger,
            ITdsTokenizer tokenizer,
            ITdsItemParser parser,
            IEnumerable<IItemFilter> itemFilters)
        {
            _logger = logger;
            _tokenizer = tokenizer;
            _parser = parser;
            _itemFilters = itemFilters.ToArray();
        }

        public async Task<Item[]> ParseFile(ScopedRagBuilder<string> scopedRagBuilder, FileSet fileSet, ItemFile file)
        {
            var rawItem = await File.ReadAllTextAsync(file.Path);
            return ParseItems(scopedRagBuilder, fileSet, file, rawItem)
                   .WhereNotNull()
                   .Where(i => _itemFilters.All(f => f.Accept(i)))
                   .ToArray();
        }

        private ImmutableDictionary<HintTypes, string> ParseHints(ItemFile file)
        {
            if (!file.Properties.ContainsKey("CodeGenNamespace"))
            {
                return ImmutableDictionary<HintTypes, string>.Empty;
            }

            return new Dictionary<HintTypes, string>
            {
                { HintTypes.Namespace, file.Properties["CodeGenNamespace"] }
            }.ToImmutableDictionary();
        }

        private Item[] ParseItems(ScopedRagBuilder<string> scopedRagBuilder, FileSet fileSet, ItemFile file, string rawItem)
        {
            try
            {
                var hints = ParseHints(file);
                var tokens = _tokenizer.Tokenize(rawItem);
                return _parser.ParseTokens(tokens)
                              .Select(i => new Item(
                                  hints,
                                  i.Id,
                                  i.Name,
                                  i.Parent,
                                  i.Path,
                                  file.Path,
                                  fileSet.Id,
                                  i.SharedFields,
                                  i.TemplateId,
                                  i.TemplateName,
                                  i.Versions))
                              .ToArray();
            }
            catch (ParseException ex)
            {
                _logger.LogError(ex, $"Could not parse file tokens {file.Path}");
                scopedRagBuilder.AddFail("Could not parse file tokens.");
            }
            catch (TokenisationException ex)
            {
                _logger.LogError(ex, $"Could not tokenise file {file.Path}");
                scopedRagBuilder.AddFail("Could not tokenise files.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error reading file {file.Path}");
                scopedRagBuilder.AddFail("Unexpected error reading file.");
            }

            return Array.Empty<Item>();
        }
    }
}