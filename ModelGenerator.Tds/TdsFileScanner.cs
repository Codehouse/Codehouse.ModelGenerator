using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.FileScanning;
using ModelGenerator.Framework.Progress;

namespace ModelGenerator.Tds
{
    public class TdsFileScanner : IFileScanner
    {
        private static class ElementNames
        {
            public static string ProjectId => "ProjectGuid";
            public static string ProjectName => "Name";
            public static string ProjectSourcePath => "SourceWebPhysicalPath";
        }

        private static class TagNames
        {
            public static XName CodeGenReference => _ns + "CodeGenReferencedProject";
            public static XName Item => _ns + "SitecoreItem";
            public static XName ItemGroup => _ns + "ItemGroup";
            public static XName PropertyGroup => _ns + "PropertyGroup";

            private static readonly XNamespace _ns = "http://schemas.microsoft.com/developer/msbuild/2003";
        }

        private readonly IFilePathFilter[] _filePathFilters;

        private readonly ILogger<TdsFileScanner> _logger;


        public TdsFileScanner(
            ILogger<TdsFileScanner> logger,
            IEnumerable<IFilePathFilter> filePathFilters)
        {
            _logger = logger;
            _filePathFilters = filePathFilters.ToArray();
        }

        public async Task<FileSet?> ScanSourceAsync(RagBuilder<string> ragBuilder, string path)
        {
            _logger.LogInformation($"Scanning source {path}");
            var projectName = Path.GetFileNameWithoutExtension(path);
            var scopedBuilder = new ScopedRagBuilder<string>(projectName);
            
            var fileset = await ReadTdsProject(scopedBuilder, projectName, path);
            if (scopedBuilder.CanPass)
            {
                scopedBuilder.AddPass();
            }
            
            ragBuilder.MergeBuilder(scopedBuilder);
            return fileset;
        }

        private ItemFile? CreateItemFile(ScopedRagBuilder<string> ragBuilder, string projectFolder, XElement element)
        {
            var path = DecodeFilePath(element.Attribute("Include")?.Value);
            if (string.IsNullOrEmpty(path))
            {
                _logger.LogWarning("Empty item file path in TDS project.");
                ragBuilder.AddWarn($"TDS item file was empty: {path}");
                return null;
            }

            var properties = ToDictionarySafe(element.Elements()
                                                     .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                                                     .Select(e => KeyValuePair.Create(e.Name.LocalName, e.Value.Trim())));

            return new ItemFile(Path.Combine(projectFolder, path), properties);
        }

        private string DecodeFilePath(string s)
        {
            return Regex.Replace(s, "%([0-9a-fA-F]{2})", match => DecodeValue(match.Groups[1].Value));
        }

        private string DecodeValue(string value)
        {
            return ((char)byte.Parse(value, NumberStyles.HexNumber)).ToString();
        }

        private bool EnsureItemFileExists(ScopedRagBuilder<string> scopedRagBuilder, string projectFolder, string itemFilePath)
        {
            if (string.IsNullOrEmpty(itemFilePath))
            {
                return false;
            }

            if (File.Exists(itemFilePath))
            {
                return true;
            }

            var relativeItemFilePath = Path.GetRelativePath(projectFolder, itemFilePath);
            _logger.LogWarning($"Item file {itemFilePath} does not exist.");
            scopedRagBuilder.AddWarn($"Item file {relativeItemFilePath} does not exist.");
            return false;
        }

        private TValue GetDictionaryKey<TKey, TValue>(string scope, IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            throw new FileScanException($"Property dictionary for {scope} did not contain entry for {key}.");
        }

        private async Task<FileSet?> ReadTdsProject(ScopedRagBuilder<string> ragBuilder, string projectName, string projectFilePath)
        {
            _logger.LogDebug($"Reading {projectName}");
            try
            {
                var projectFolder = Path.GetDirectoryName(projectFilePath);
                if (projectFolder == null)
                {
                    throw new FileScanException($"Could not resolve path from project {projectFolder}");
                }

                using var xmlReader = XmlReader.Create(projectFilePath, new XmlReaderSettings { Async = true });
                var xml = await XDocument.LoadAsync(xmlReader, LoadOptions.None, CancellationToken.None);
                var properties = ToDictionarySafe(xml.Root
                                                     .Elements(TagNames.PropertyGroup)
                                                     .Where(e => !e.HasAttributes)
                                                     .Elements()
                                                     .Select(e => KeyValuePair.Create(e.Name.LocalName, e.Value.Trim())));

                var files = xml.Root
                               .Elements(TagNames.ItemGroup)
                               .Elements(TagNames.Item)
                               .Select(e => CreateItemFile(ragBuilder, projectFolder, e))
                               .Where(f => f != null)
                               .Where(f => EnsureItemFileExists(ragBuilder, projectFolder, f.Path))
                               .Where(f => _filePathFilters.All(filter => filter.Accept(f.Path)))
                               .ToImmutableList();

                var references = xml.Root
                                    .Elements(TagNames.ItemGroup)
                                    .Elements(TagNames.CodeGenReference)
                                    .Select(e => e.Value)
                                    .ToImmutableArray();

                return new FileSet
                {
                    Files = files,
                    Id = GetDictionaryKey(projectName, properties, ElementNames.ProjectId),
                    Name = GetDictionaryKey(projectName, properties, ElementNames.ProjectName),
                    Namespace = GetDictionaryKey(projectName, properties, ElementNames.ProjectName)
                        .Replace(".Master", ".Models"),
                    ItemPath = projectFolder,
                    ModelPath = Path.GetFullPath(Path.Combine(projectFolder, GetDictionaryKey(projectName, properties, ElementNames.ProjectSourcePath))),
                    References = references
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not parse file {projectName}");
                ragBuilder.AddWarn("Could not parse TDS project file.", ex);
                return null;
            }
        }

        private IDictionary<string, string> ToDictionarySafe(IEnumerable<KeyValuePair<string, string>> properties)
        {
            Dictionary<string, string> Aggregator(Dictionary<string, string> d, KeyValuePair<string, string> kvp)
            {
                if (d.ContainsKey(kvp.Key))
                {
                    var currentValue = d[kvp.Key];
                    if (string.IsNullOrWhiteSpace(currentValue))
                    {
                        d[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    d.Add(kvp.Key, kvp.Value);
                }

                return d;
            }

            return properties.Aggregate(
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                Aggregator);
        }
    }
}