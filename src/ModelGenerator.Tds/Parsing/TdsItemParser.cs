using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ModelGenerator.Framework.Configuration;
using ModelGenerator.Framework.FileParsing;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace ModelGenerator.Tds.Parsing
{
    internal class TdsItemParser : ITdsItemParser
    {
        public TokenListParser<TdsItemTokens, TdsItem[]> TdsFile =>
            TdsItem.Many()
                   .AtEnd();

        private TokenListParser<TdsItemTokens, Field> TdsField =>
            from startField in Token.EqualTo(TdsItemTokens.FieldSeparator)
                                    .IgnoreThen(Token.EqualTo(TdsItemTokens.NewLine))
            from properties in TdsProperties
            from value in TdsFieldValue
            select CreateField(properties, value);

        private TokenListParser<TdsItemTokens, string> TdsFieldValue =>
            from lines in Token.EqualTo(TdsItemTokens.Content).Try()
                               .Or(Token.EqualTo(TdsItemTokens.NewLine).Try())
                               .Many()
            select GetFieldValue(lines);

        private TokenListParser<TdsItemTokens, TdsItem> TdsItem =>
            from begin in Token.EqualTo(TdsItemTokens.ItemSeparator)
                               .IgnoreThen(Token.EqualTo(TdsItemTokens.NewLine))
            from itemProperties in TdsProperties
            from sharedFields in TdsField.Many()
            from versions in TdsVersion.Many()
            select CreateItem(itemProperties, sharedFields, versions);

        private TokenListParser<TdsItemTokens, Dictionary<string, string>> TdsProperties =>
            from properties in Token.EqualTo(TdsItemTokens.PropertyName)
                                    .Apply(_tokenizer.PropertyName)
                                    .Then(name =>
                                        // Have found some properties that are missing a value, so made content optional.
                                        Token.EqualTo(TdsItemTokens.Content).OptionalOrDefault(Token<TdsItemTokens>.Empty)
                                             .Then(value =>
                                                 Token.EqualTo(TdsItemTokens.NewLine)
                                                      .Select(_ => KeyValuePair.Create(
                                                          name.ToStringValue(),
                                                          // As content is optional, must check if it's empty first.
                                                          value.HasValue ? value.ToStringValue() : string.Empty))))
                                    .Many()
            from separator in Token.EqualTo(TdsItemTokens.NewLine)
            select new Dictionary<string, string>(properties, StringComparer.OrdinalIgnoreCase);

        private TokenListParser<TdsItemTokens, LanguageVersion> TdsVersion =>
            from begin in Token.EqualTo(TdsItemTokens.VersionSeparator)
                               .IgnoreThen(Token.EqualTo(TdsItemTokens.NewLine))
            from versionProperties in TdsProperties
            from fields in TdsField.Many()
            select CreateVersion(versionProperties, fields);

        private readonly ItemParsingSettings _settings;
        private readonly ITdsTokenizer _tokenizer;

        public TdsItemParser(ItemParsingSettings settings, ITdsTokenizer tokenizer)
        {
            _settings = settings;
            _tokenizer = tokenizer;
        }

        public TdsItem[] ParseTokens(TokenList<TdsItemTokens> tokenList)
        {
            var parsed = TdsFile.TryParse(tokenList);
            if (!parsed.HasValue)
            {
                throw new TdsParseException(parsed.ErrorPosition, "Could not parse TDS item file: " + parsed.ErrorMessage);
            }

            return parsed.Value;
        }

        private static string GetFieldValue(Token<TdsItemTokens>[] lines)
        {
            return string.Join("", lines.SkipLast(1).Select(t => t.ToStringValue()));
        }

        private Field CreateField(Dictionary<string, string> properties, string value)
        {
            var name = string.Intern(properties[TdsPropertyNames.Name]);
            if (_settings.InternedFieldValues.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                value = string.Intern(value);
            }

            return new Field(Guid.Parse(properties[TdsPropertyNames.FieldId]), name, value);
        }

        private TdsItem CreateItem(Dictionary<string, string> itemProperties, Field[] sharedFields, LanguageVersion[] versions)
        {
            var id = Guid.Parse(itemProperties[TdsPropertyNames.Id]);
            var parent = Guid.Parse(itemProperties[TdsPropertyNames.ParentId]);
            var templateId = Guid.Parse(itemProperties[TdsPropertyNames.TemplateId]);

            return new TdsItem(
                id,
                itemProperties[TdsPropertyNames.Name],
                parent,
                itemProperties[TdsPropertyNames.Path],
                FilterIncludedFields(sharedFields).ToImmutableList(),
                templateId,
                string.Intern(itemProperties[TdsPropertyNames.TemplateKey]),
                versions.ToImmutableList()
            );
        }

        private LanguageVersion CreateVersion(Dictionary<string, string> versionProperties, Field[] fields)
        {
            return new LanguageVersion(
                FilterIncludedFields(fields).ToImmutableDictionary(f => f.Id),
                string.Intern(versionProperties[TdsPropertyNames.Language]),
                int.Parse(versionProperties[TdsPropertyNames.Version]),
                Guid.Parse(versionProperties[TdsPropertyNames.Revision])
            );
        }

        private IEnumerable<Field> FilterIncludedFields(IEnumerable<Field> fields)
        {
            return fields
                .Where(f => _settings.IncludedFields.Contains(f.Name, StringComparer.OrdinalIgnoreCase));
        }
    }
}