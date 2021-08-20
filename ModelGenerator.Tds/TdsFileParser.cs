using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Tds.Parsing;

namespace ModelGenerator.Tds
{
    internal class TdsFileParser : IFileParser
    {
        private readonly ILogger<TdsFileParser> _logger;
        private readonly ITdsTokenizer _tokenizer;
        private readonly ITdsItemParser _parser;

        public TdsFileParser(ILogger<TdsFileParser> logger, ITdsTokenizer tokenizer, ITdsItemParser parser)
        {
            _logger = logger;
            _tokenizer = tokenizer;
            _parser = parser;
        }
        
        public async IAsyncEnumerable<Item> ParseFile(string filePath)
        {
            var rawItem = await File.ReadAllTextAsync(filePath);
            
            var tokens = _tokenizer.Tokenize(rawItem);
            yield return _parser.ParseTokens(tokens);
        }
    }
}