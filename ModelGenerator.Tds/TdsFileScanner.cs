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

namespace ModelGenerator.Tds
{
    public class TdsFileScanner : IFileScanner
    {
        private static class ElementNames
        {
            public static string EnableGeneration => "EnableCodeGeneration";
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

        public async Task<FileSet?> ScanSourceAsync(string path)
        {
            _logger.LogInformation($"Scanning source {path}");
            return await ReadTdsProject(path);
        }

        private string DecodeFilePath(string s)
        {
            return Regex.Replace(s, "%([0-9a-fA-F]{2})", match => DecodeValue(match.Groups[1].Value));
        }

        private string DecodeValue(string value)
        {
            return ((char)byte.Parse(value, NumberStyles.HexNumber)).ToString();
        }

        private bool EnsureItemFileExists(string itemFilePath)
        {
            if (string.IsNullOrEmpty(itemFilePath))
            {
                return false;
            }
            
            if (File.Exists(itemFilePath))
            {
                return true;
            }

            _logger.LogWarning($"Item file {itemFilePath} does not exist.");
            return false;
        }

        private async Task<FileSet> ReadTdsProject(string projectFilePath)
        {
            var projectName = Path.GetFileNameWithoutExtension(projectFilePath);
            _logger.LogDebug($"Reading {projectName}");
            try
            {
                var projectFolder = Path.GetDirectoryName(projectFilePath);
                if (projectFolder == null)
                {
                    throw new InvalidOperationException($"Could not resolve path from project {projectFolder}");
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
                               .Select(e => CreateItemFile(projectFolder, e))
                               .Where(f => f != null)
                               .Where(f => EnsureItemFileExists(f.Path))
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
                    Id = properties[ElementNames.ProjectId],
                    Name = properties[ElementNames.ProjectName],
                    Namespace = properties[ElementNames.ProjectName].Replace(".Master", ".Models"),
                    ItemPath = projectFolder,
                    ModelPath = Path.GetFullPath(Path.Combine(projectFolder, properties[ElementNames.ProjectSourcePath])),
                    References = references
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not parse file {projectName}");
                return null;
            }
        }

        private ItemFile? CreateItemFile(string projectFolder, XElement element)
        {
            var path = DecodeFilePath(element.Attribute("Include")?.Value);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var properties = ToDictionarySafe(element.Elements()
                                    .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                                    .Select(e => KeyValuePair.Create(e.Name.LocalName, e.Value.Trim())));

            return new ItemFile(Path.Combine(projectFolder, path), properties);
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