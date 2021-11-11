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

        public async Task<Item[]> ParseFile(FileSet fileSet, ItemFile file)
        {
            var rawItem = await File.ReadAllTextAsync(file.Path);
            return ParseItems(fileSet, file, rawItem)
                .WhereNotNull()
                .Where(i => _itemFilters.All(f => f.Accept(i)))
                .ToArray();
        }

        private Item[] ParseItems(FileSet fileSet, ItemFile file, string rawItem)
        {
            try
            {
                var hints = ParseHints(file);
                var tokens = _tokenizer.Tokenize(rawItem);
                return _parser.ParseTokens(tokens)
                              .Select(i => i with { Hints = hints, RawFilePath = file.Path, SetId = fileSet.Id })
                              .ToArray();
            }
            catch (ParseException ex)
            {
                _logger.LogError(ex, $"Could not parse file tokens {file.Path}");
            }
            catch (TokenisationException ex)
            {
                _logger.LogError(ex, $"Could not tokenise file {file.Path}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error reading file {file.Path}");
            }

            return Array.Empty<Item>();
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
    }
}