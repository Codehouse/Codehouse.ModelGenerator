using System;
using System.Collections.Generic;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace ModelGenerator.Tds.Parsing
{
    public class TdsItemTokenizer : Tokenizer<TdsItemTokens>, ITdsTokenizer
    {
        private TextParser<TextSpan> ContentParser => Span.WithoutAny(c => c == '\r' || c == '\n')
                                                          .Then(s => NewLineParser);
        private TextParser<TextSpan> FieldSeparatorParser => SeparatorParser
                                                             .Then(s => Span.EqualTo("field"))
                                                             .Then(s => SeparatorParser);

        private TextParser<TextSpan> ItemSeparatorParser => SeparatorParser
                                                            .Then(s => Span.EqualTo("item"))
                                                            .Then(s => SeparatorParser);
        private TextParser<TextSpan> VersionSeparatorParser => SeparatorParser
                                                               .Then(s => Span.EqualTo("version"))
                                                               .Then(s => SeparatorParser);

        private TextParser<TextSpan> PropertyNameParser => Span.WithoutAny(c => c == ':');

        private TextParser<TextSpan> PropertySeparatorParser => Span.EqualTo(":").Then(c => Span.WhiteSpace);

        private TextParser<TextSpan> PropertyValueParser => Span.WithoutAny(c => c == '\r' || c == '\n');

        private TextParser<TextSpan> SeparatorParser => Span.EqualTo("----");

        private TextParser<TextSpan> NewLineParser => Span.EqualTo("\r\n")
                                                   .Or(Span.EqualTo("\n\r"))
                                                   .Or(Span.EqualTo("\n"));
        
        protected override IEnumerable<Result<TdsItemTokens>> Tokenize(TextSpan span, TokenizationState<TdsItemTokens> state)
        {
            var next = SkipWhiteSpace(span);
            if (!next.HasValue)
            {
                yield break;
            }

            var expectContent = false;
            do
            {
                if (next.Value == '-')
                {
                    var itemSeparator = ItemSeparatorParser(next.Location);
                    if (itemSeparator.HasValue)
                    {
                        next = Skip(itemSeparator.Remainder.ConsumeChar(), NewLineParser);
                        yield return Result.Value(TdsItemTokens.ItemSeparator, itemSeparator.Location, itemSeparator.Remainder);
                        expectContent = false;
                        continue;
                    }
                    
                    var fieldSeparator = FieldSeparatorParser(next.Location);
                    if (fieldSeparator.HasValue)
                    {
                        next = Skip(fieldSeparator.Remainder.ConsumeChar(), NewLineParser);
                        yield return Result.Value(TdsItemTokens.FieldSeparator, fieldSeparator.Location, fieldSeparator.Remainder);
                        expectContent = false;
                        continue;
                    }
                    
                    var versionSeparator = VersionSeparatorParser(next.Location);
                    if (versionSeparator.HasValue)
                    {
                        next = Skip(versionSeparator.Remainder.ConsumeChar(), NewLineParser);
                        yield return Result.Value(TdsItemTokens.VersionSeparator, versionSeparator.Location, versionSeparator.Remainder);
                        expectContent = false;
                        continue;
                    }
                }

                var newLine = NewLineParser(next.Location);
                if (newLine.HasValue)
                {
                    next = newLine.Remainder.ConsumeChar();
                    expectContent = true;
                    continue;
                }

                if (expectContent)
                {
                    var content = ContentParser(next.Location);
                    if (content.HasValue)
                    {
                        next = content.Remainder.ConsumeChar();
                        yield return Result.Value(TdsItemTokens.FieldValue, content.Location, content.Remainder);
                    }
                }
                else
                {
                    var propertyName = PropertyNameParser(next.Location);
                    if (propertyName.HasValue)
                    {
                        var intermediate = Skip(propertyName.Remainder.ConsumeChar(), PropertySeparatorParser);
                        var propertyValue = PropertyValueParser(intermediate.Location);
                        if (propertyValue.HasValue)
                        {
                            next = Skip(propertyValue.Remainder.ConsumeChar(), NewLineParser);
                            
                            yield return Result.Value(TdsItemTokens.PropertyName, propertyName.Location, propertyName.Remainder);
                            yield return Result.Value(TdsItemTokens.PropertyValue, propertyValue.Location, propertyValue.Remainder);
                        }
                        else
                        {
                            throw new InvalidOperationException("Could not tokenise file: expected property value.");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Could not tokenise file: expected property name.");
                    }
                }
            } while (next.HasValue);
        }

        private Result<char> Skip(Result<char> input, TextParser<TextSpan> parser)
        {
            var result = parser.Invoke(input.Location);
            if (!result.HasValue)
            {
                throw new InvalidOperationException("Could not skip expected token");
            }

            return result.Remainder.ConsumeChar();
        }
    }
}