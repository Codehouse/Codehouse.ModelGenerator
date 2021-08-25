using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ModelGenerator.Framework.FileParsing;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using ParseException = ModelGenerator.Framework.FileParsing.ParseException;

namespace ModelGenerator.Tds.Parsing
{
    internal class TdsItemParser : ITdsItemParser
    {
        public TokenListParser<TdsItemTokens, Item[]> TdsFile =>
            TdsItem.Many()
                    .AtEnd();

        private TokenListParser<TdsItemTokens, Field> TdsField =>
            from startField in Token.EqualTo(TdsItemTokens.FieldSeparator)
                                    .IgnoreThen(Token.EqualTo(TdsItemTokens.NewLine))
            from properties in TdsProperties
            from value in TdsFieldValue
            select CreateField(properties, value);

        private TokenListParser<TdsItemTokens, Item> TdsItem =>
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

        private TokenListParser<TdsItemTokens, string> TdsFieldValue =>
            from lines in Token.EqualTo(TdsItemTokens.Content).Try()
                               .Or(Token.EqualTo(TdsItemTokens.NewLine))
                               .Many()
            select GetFieldValue(lines);
        
        private readonly ITdsTokenizer _tokenizer;

        public TdsItemParser(ITdsTokenizer tokenizer)
        {
            _tokenizer = tokenizer;
        }

        public Item[] ParseTokens(TokenList<TdsItemTokens> tokenList)
        {
            var parsed = TdsFile.TryParse(tokenList);
            if (!parsed.HasValue)
            {
                ////value = null;
                ////error = parsed.ToString();
                ////errorPosition = parsed.ErrorPosition;
                throw new ParseException("Could not parse TDS item file: " + parsed.ErrorMessage); 
            }

            return parsed.Value;
        }


        private static Field CreateField(Dictionary<string, string> properties, string value)
        {
            return new Field
            {
                Id = Guid.Parse(properties[TdsPropertyNames.FieldId]),
                Name = properties[TdsPropertyNames.Name],
                Value = value
            };
        }

        private static Item CreateItem(Dictionary<string, string> itemProperties, Field[] sharedFields, LanguageVersion[] versions)
        {
            var id = Guid.Parse(itemProperties[TdsPropertyNames.Id]);
            var parent = Guid.Parse(itemProperties[TdsPropertyNames.ParentId]);
            var templateId = Guid.Parse(itemProperties[TdsPropertyNames.TemplateId]);

            return new Item
            {
                Id = id,
                Name = itemProperties[TdsPropertyNames.Name],
                Parent = parent,
                Path = itemProperties[TdsPropertyNames.Path],
                TemplateId = templateId,
                TemplateName = itemProperties[TdsPropertyNames.TemplateKey],
                SharedFields = sharedFields.ToImmutableList(),
                Versions = versions.ToImmutableList()
            };
        }

        private static LanguageVersion CreateVersion(Dictionary<string, string> versionProperties, Field[] fields)
        {
            return new LanguageVersion
            {
                Language = versionProperties[TdsPropertyNames.Language],
                Number = int.Parse(versionProperties[TdsPropertyNames.Version]),
                Revision = Guid.Parse(versionProperties[TdsPropertyNames.Revision]),
                Fields = fields.ToImmutableDictionary(f => f.Id)
            };
        }

        private static string GetFieldValue(Token<TdsItemTokens>[] lines)
        {
            return string.Empty;
        }
    }
}