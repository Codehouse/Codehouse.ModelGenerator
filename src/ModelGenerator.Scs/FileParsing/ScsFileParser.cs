using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Framework.FileScanning;
using ModelGenerator.Framework.Progress;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ModelGenerator.Scs.FileParsing
{
    public class ScsFileParser : IFileParser
    {
        private readonly IDeserializer _deserialiser;
        private readonly IItemFilter[] _itemFilters;
        private readonly ILogger<ScsFileParser> _log;

        public ScsFileParser(ILogger<ScsFileParser> log, IEnumerable<IItemFilter> itemFilters)
        {
            _deserialiser = new DeserializerBuilder()
                            .WithNamingConvention(PascalCaseNamingConvention.Instance)
                            .Build();
            _itemFilters = itemFilters.ToArray();
            _log = log;
        }

        public async Task<Item[]> ParseFile(ScopedRagBuilder<string> scopedRagBuilder, FileSet fileSet, ItemFile file)
        {
            var scsItem = await ReadFile(file, scopedRagBuilder);
            if (scsItem == null)
            {
                return Array.Empty<Item>();
            }

            var item = ConvertScsItem(scsItem, scopedRagBuilder, fileSet, file);
            return _itemFilters.All(f => f.Accept(scopedRagBuilder, item))
                       ? new[] {item}
                       : Array.Empty<Item>();
        }

        private Field ConvertScsField(ScsField scsField, ScopedRagBuilder<string> scopedRagBuilder)
        {
            return new Field(scsField.Id, scsField.Name, scsField.Value);
        }

        private IEnumerable<Field> ConvertScsFields(IEnumerable<ScsField> scsFields, ScopedRagBuilder<string> scopedRagBuilder)
        {
            return scsFields.Select(f => ConvertScsField(f, scopedRagBuilder));
        }

        private Item ConvertScsItem(ScsItem scsItem, ScopedRagBuilder<string> scopedRagBuilder, FileSet fileSet, ItemFile file)
        {
            var sharedFields = ConvertScsFields(scsItem.SharedFields, scopedRagBuilder)
                .ToImmutableList();

            // TODO: SCS is missing version revision
            // TODO: Item model completely ignores distinction between shared and unversioned fields.
            var versions = scsItem.Languages
                                  .SelectMany(l => l.Versions, (l, v) => new
                                  {
                                      l.Language,
                                      v.Version,
                                      Fields = ConvertScsFields(v.Fields.Union(l.Fields), scopedRagBuilder).ToImmutableDictionary(f => f.Id)
                                  })
                                  .Select(v => new LanguageVersion(v.Fields, v.Language, v.Version, Guid.Empty))
                                  .ToImmutableList();

            // TODO: SCS is missing template name
            return new Item(ImmutableDictionary<HintTypes, string>.Empty, scsItem.Id, scsItem.Name, scsItem.Parent,
                scsItem.Path, file.Path, fileSet.Id, sharedFields, scsItem.Template, null!, versions);
        }

        private async Task<ScsItem?> ReadFile(ItemFile file, ScopedRagBuilder<string> scopedRagBuilder)
        {
            try
            {
                var fileContent = await File.ReadAllTextAsync(file.Path);
                return _deserialiser.Deserialize<ScsItem>(fileContent);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Could not parse SCS item file {file}.", file.Path);
                scopedRagBuilder.AddFail($"Could not parse file {file.Path}", ex);
                return null;
            }
        }
    }
}