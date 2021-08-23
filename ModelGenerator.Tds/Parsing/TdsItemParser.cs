using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ModelGenerator.Framework.FileParsing;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace ModelGenerator.Tds.Parsing
{
    public class TdsItemParser : ITdsItemParser
    {
        private static class Parsers
        {
            public static TokenListParser<TdsItemTokens, Item[]> TdsFile =>
                _tdsItem.Many();
            
            private static readonly TokenListParser<TdsItemTokens, Field> _tdsField =
                from startField in Token.EqualTo(TdsItemTokens.FieldSeparator)
                from properties in _tdsProperties
                from value in _tdsFieldValue
                select CreateField(properties, value);

            private static readonly TokenListParser<TdsItemTokens, string> _tdsFieldValue =
                from lines in Token.EqualTo(TdsItemTokens.FieldValue).Many()
                select GetFieldValue(lines);

            private static readonly TokenListParser<TdsItemTokens, Item> _tdsItem =
                from begin in Token.EqualTo(TdsItemTokens.ItemSeparator)
                from itemProperties in _tdsProperties
                from sharedFields in _tdsField.Many()
                from versions in _tdsVersion.Many()
                select CreateItem(itemProperties, sharedFields, versions);

            private static readonly TokenListParser<TdsItemTokens, Dictionary<string, string>> _tdsProperties =
                from properties in Token.EqualTo(TdsItemTokens.PropertyName)
                                        .Then(name => Token
                                                      .EqualTo(TdsItemTokens.PropertyValue)
                                                      .Select(value => KeyValuePair.Create(name.ToStringValue(), value.ToStringValue())))
                                        .Many()
                select new Dictionary<string, string>(properties, StringComparer.OrdinalIgnoreCase);

            private static readonly TokenListParser<TdsItemTokens, LanguageVersion> _tdsVersion =
                from begin in Token.EqualTo(TdsItemTokens.ItemSeparator)
                from versionProperties in _tdsProperties
                from fields in _tdsField.Many()
                select CreateVersion(versionProperties, fields);
        }

        private static class PropertyNames
        {
            public const string Database = "database";
            public const string FieldId = "field";
            public const string Id = "id";
            public const string Key = "key";
            public const string Length = "content-length";
            public const string Language = "language";
            public const string Name = "name";
            public const string ParentId = "parent";
            public const string Path = "path";
            public const string Revision = "revision";
            public const string TemplateId = "template";
            public const string TemplateKey = "templatekey";
            public const string Version = "version";
        }

        public Item[] ParseTokens(TokenList<TdsItemTokens> tokenList)
        {
            var parsed = Parsers.TdsFile.TryParse(tokenList);
            if (!parsed.HasValue)
            {
                ////value = null;
                ////error = parsed.ToString();
                ////errorPosition = parsed.ErrorPosition;
                throw new InvalidOperationException("Could not parse TDS item file");
            }

            //value = parsed.Value;
            //error = null;
            //errorPosition = Position.Empty;
            return parsed.Value;
        }


        private static Field CreateField(Dictionary<string, string> properties, string value)
        {
            // TODO: fill in properties name,key

            return new Field
            {
                Id = Guid.Parse(properties[PropertyNames.FieldId]),
                Value = value
            };
        }

        private static Item CreateItem(Dictionary<string, string> itemProperties, Field[] sharedFields, LanguageVersion[] versions)
        {
            var id = Guid.Parse(itemProperties[PropertyNames.Id]);
            var parent = Guid.Parse(itemProperties[PropertyNames.ParentId]);
            var templateId = Guid.Parse(itemProperties[PropertyNames.TemplateId]);
            
            return new Item
            {
                Id = id,
                Name = itemProperties[PropertyNames.Name],
                Parent = parent,
                Path = itemProperties[PropertyNames.Path],
                TemplateId = templateId,
                TemplateName = itemProperties[PropertyNames.TemplateKey],
                SharedFields = sharedFields.ToImmutableList(),
                Versions = versions.ToImmutableList()
            };
        }

        private static LanguageVersion CreateVersion(Dictionary<string, string> versionProperties, Field[] fields)
        {
            return new LanguageVersion
            {
                Language = versionProperties[PropertyNames.Language],
                Number = int.Parse(versionProperties[PropertyNames.Version]),
                Revision = Guid.Parse(versionProperties[PropertyNames.Revision]), 
                Fields = fields.ToImmutableDictionary(f => f.Id)
            };
        }

        private static string GetFieldValue(Token<TdsItemTokens>[] lines)
        {
            return string.Empty;
        }
    }
}