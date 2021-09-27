using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.FileParsing;
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

        public async IAsyncEnumerable<Item> ParseFile(string filePath)
        {
            var rawItem = await File.ReadAllTextAsync(filePath);
            var parsedItems = ParsedItems(filePath, rawItem);

            foreach (var item in parsedItems)
            {
                if (_itemFilters.All(f => f.Accept(item)))
                {
                    yield return item;
                }
            }
        }

        private Item[] ParsedItems(string filePath, string rawItem)
        {
            try
            {
                var tokens = _tokenizer.Tokenize(rawItem);
                return _parser.ParseTokens(tokens)
                              .Select(i => i with { RawFilePath = filePath })
                              .ToArray();
            }
            catch (ParseException ex)
            {
                _logger.LogError(ex, $"Could not parse file tokens {filePath}");
            }
            catch (TokenisationException ex)
            {
                _logger.LogError(ex, $"Could not tokenise file {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error reading file {filePath}");
            }

            return new Item[0];
        }
    }
}