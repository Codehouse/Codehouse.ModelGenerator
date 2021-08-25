using System;
using System.Linq;
using System.Reflection;
using ModelGenerator.Framework.FileParsing;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace ModelGenerator.Tds.Parsing
{
    public class TdsItemTokenizer : ITdsTokenizer
    {
        public TextParser<TextSpan> PropertyName { get; private set; }
        private readonly Tokenizer<TdsItemTokens> _tokenizer;

        public TdsItemTokenizer()
        {
            BuildParsers();
            _tokenizer = BuildTokenizer();
        }

        private void BuildParsers()
        {
            PropertyName = BuildPropertyNameParser(typeof(TdsPropertyNames));  
        }

        public TokenList<TdsItemTokens> Tokenize(string input)
        {
            var result = _tokenizer.TryTokenize(input);
            if (result.HasValue)
            {
                return result.Value;
            }

            throw new TokenisationException(result.ErrorMessage);
        }

        private TextParser<TextSpan> BuildPropertyNameParser(Type type)
        {
            var properties = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                                 .Select(f => f.GetValue(null))
                                 .Cast<string>()
                                 .OrderByDescending(s => s.Length)
                                 .ToArray();
            
            var parser = Span.EqualToIgnoreCase(properties.First()).Try();
            foreach (var property in properties.Skip(1))
            {
                parser = parser.Or(Span.EqualToIgnoreCase(property)).Try();
            }

            return parser.Between(Span.Length(0), Span.EqualTo(": "));
        }

        private TextParser<TextSpan> BuildSeparatorParser(string separatorType)
        {
            var separatorParser = Span.EqualTo("----");
            return separatorParser
                   .Then(_ => Span.EqualTo(separatorType))
                   .Then(_ => separatorParser);
        }

        private Tokenizer<TdsItemTokens> BuildTokenizer()
        {
            var contentParser = Span.WithoutAny(c => c == '\r' || c == '\n');
            var newLineParser = Span.EqualTo("\r\n").Try()
                                    .Or(Span.EqualTo("\n\r").Try())
                                    .Or(Span.EqualTo("\n").Try());
            
            return new TokenizerBuilder<TdsItemTokens>()
                   .Match(BuildSeparatorParser("item"), TdsItemTokens.ItemSeparator)
                   .Match(BuildSeparatorParser("version"), TdsItemTokens.VersionSeparator)
                   .Match(BuildSeparatorParser("field"), TdsItemTokens.FieldSeparator)
                   .Match(PropertyName, TdsItemTokens.PropertyName)
                   //.Match(propertySeparator, TdsItemTokens.PropertySeparator)
                   .Match(contentParser, TdsItemTokens.Content)
                   .Match(newLineParser, TdsItemTokens.NewLine)
                   .Build();
        }
    }
}