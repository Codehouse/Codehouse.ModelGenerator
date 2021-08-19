using System;
using System.Collections.Generic;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace ModelGenerator.Tds.Parsing
{
    public class TdsItemTokenizer : Tokenizer<TdsItemTokens>, ITdsTokenizer
    {
        protected override IEnumerable<Result<TdsItemTokens>> Tokenize(TextSpan span, TokenizationState<TdsItemTokens> state)
        {
            var next = SkipWhiteSpace(span);
            if (!next.HasValue)
            {
                yield break;
            }

            do
            {
                if (next.Value == '-')
                {
                    yield return LexSeparator(ref next);
                    yield return LexSeparatorType(ref next);
                    yield return LexSeparator(ref next);
                }
                else if (char.IsLetter(next.Value))
                {
                    yield return LexPropertyName(ref next);
                    LexPropertySeparator(ref next);
                    yield return LexPropertyValue(ref next);
                }
                else
                {
                    yield return LexContent(ref next);
                }
                    
            } while (next.HasValue);
        }

        private Result<TdsItemTokens> LexContent(ref Result<char> next)
        {
            return LexToken(ref next, TdsItemTokens.FieldValue, Span.MatchedBy(Character.ExceptIn('\r', '\n')), true);
        }

        private Result<TdsItemTokens> LexPropertyValue(ref Result<char> next)
        {
            return LexToken(ref next, TdsItemTokens.PropertyValue, Span.MatchedBy(Character.ExceptIn('\r', '\n')), false);
        }

        private Result<TdsItemTokens> LexPropertySeparator(ref Result<char> next)
        {
            return LexToken(ref next, TdsItemTokens.PropertySeparator, Span.MatchedBy(Character.EqualTo(':')), true);
        }

        private Result<TdsItemTokens> LexPropertyName(ref Result<char> next)
        {
            return LexToken(ref next, TdsItemTokens.PropertyName, Span.MatchedBy(Character.LetterOrDigit), false);
        }

        private Result<TdsItemTokens> LexToken(ref Result<char> next, TdsItemTokens tokenType, TextParser<TextSpan> parser, bool skipWhitespace)
        {
            var location = next.Location;
            var token = parser.Invoke(location);

            next = token.Remainder.ConsumeChar();
            if (skipWhitespace)
            {
                next = SkipWhiteSpace(next.Location);
            }
            
            return Result.Value(tokenType, location, token.Remainder);
        }
        
        private Result<TdsItemTokens> LexSeparator(ref Result<char> next)
        {
            return LexToken(ref next, TdsItemTokens.Separator, Span.MatchedBy(Character.EqualTo('-')), true);
        }
        
        private Result<TdsItemTokens> LexSeparatorType(ref Result<char> next)
        {
            return LexToken(ref next, TdsItemTokens.SeparatorType, Span.MatchedBy(Character.Letter), true);
        }

        private Result<char> SkipNewLine(Result<char> next)
        {
            throw new NotImplementedException();
        }
    }
}