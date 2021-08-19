using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Tds.Parsing;

namespace ModelGenerator.Tds
{
    internal class TdsFileParser : IFileParser
    {
        public async Task<IEnumerable<Item>> ParseFile(string filePath)
        {
            var rawItem = await File.ReadAllTextAsync(filePath);
            var tokenizer = new TdsItemTokenizer();
            var tokens = tokenizer.Tokenize(rawItem).ToArray();

            foreach (var token in tokens)
            {
                Console.WriteLine($"{token.Kind} - {token.Span.ToStringValue()}");
            }

            throw new Exception("End");
        }
    }
}