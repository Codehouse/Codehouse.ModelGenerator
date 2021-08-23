using System;
using System.Collections.Generic;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace ModelGenerator.Tds.Parsing
{
    public class TdsItemTokenizer : Tokenizer<TdsItemTokens>, ITdsTokenizer
    {
        private static readonly TextParser<TextSpan> _contentParser = Span.WithoutAny(c => c == '\r' || c == '\n')
                                                                          .Then(s => Parse.Ref(() => _newLineParser));

        private static readonly TextParser<TextSpan> _fieldSeparatorParser = Parse.Ref(() => _separatorParser)
                                                                                  .Then(s => Span.EqualTo("field"))
                                                                                  .Then(s => Parse.Ref(() => _separatorParser));

        private static readonly TextParser<TextSpan> _itemSeparatorParser = Parse.Ref(() => _separatorParser)
                                                                                 .Then(s => Span.EqualTo("item"))
                                                                                 .Then(s => Parse.Ref(() => _separatorParser));

        private static readonly TextParser<TextSpan> _newLineParser = Span.EqualTo("\r\n")
                                                                          .Or(Span.EqualTo("\n\r"))
                                                                          .Or(Span.EqualTo("\n"));

        private static readonly TextParser<TextSpan> _propertyNameParser = Span.WithoutAny(c => c == ':');

        private static readonly TextParser<TextSpan> _propertySeparatorParser = Span.EqualTo(":")
                                                                                    .Then(c => Span.WhiteSpace);

        private static readonly TextParser<TextSpan> _propertyValueParser = Span.WithoutAny(c => c == '\r' || c == '\n');

        private static readonly TextParser<TextSpan> _versionSeparatorParser = Parse.Ref(() => _separatorParser)
                                                                                    .Then(s => Span.EqualTo("version"))
                                                                                    .Then(s => Parse.Ref(() => _separatorParser));
        
        private static readonly TextParser<TextSpan> _separatorParser = Span.EqualTo("----");

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
                    var itemSeparator = _itemSeparatorParser(next.Location);
                    if (itemSeparator.HasValue)
                    {
                        next = Skip(itemSeparator.Remainder.ConsumeChar(), _newLineParser);
                        yield return Result.Value(TdsItemTokens.ItemSeparator, itemSeparator.Location, itemSeparator.Remainder);
                        expectContent = false;
                        continue;
                    }

                    var fieldSeparator = _fieldSeparatorParser(next.Location);
                    if (fieldSeparator.HasValue)
                    {
                        next = Skip(fieldSeparator.Remainder.ConsumeChar(), _newLineParser);
                        yield return Result.Value(TdsItemTokens.FieldSeparator, fieldSeparator.Location, fieldSeparator.Remainder);
                        expectContent = false;
                        continue;
                    }

                    var versionSeparator = _versionSeparatorParser(next.Location);
                    if (versionSeparator.HasValue)
                    {
                        next = Skip(versionSeparator.Remainder.ConsumeChar(), _newLineParser);
                        yield return Result.Value(TdsItemTokens.VersionSeparator, versionSeparator.Location, versionSeparator.Remainder);
                        expectContent = false;
                        continue;
                    }
                }

                var newLine = _newLineParser(next.Location);
                if (newLine.HasValue)
                {
                    next = newLine.Remainder.ConsumeChar();
                    expectContent = true;
                    continue;
                }

                if (expectContent)
                {
                    var content = _contentParser(next.Location);
                    if (content.HasValue)
                    {
                        next = content.Remainder.ConsumeChar();
                        yield return Result.Value(TdsItemTokens.FieldValue, content.Location, content.Remainder);
                    }
                }
                else
                {
                    var propertyName = _propertyNameParser(next.Location);
                    if (propertyName.HasValue)
                    {
                        var intermediate = Skip(propertyName.Remainder.ConsumeChar(), _propertySeparatorParser);
                        var propertyValue = _propertyValueParser(intermediate.Location);
                        if (propertyValue.HasValue)
                        {
                            next = Skip(propertyValue.Remainder.ConsumeChar(), _newLineParser);

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