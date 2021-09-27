using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using GlobExpressions;
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
            public static XName PropertyGroup => _ns + "PropertyGroup";
            public static XName ItemGroup => _ns + "ItemGroup";
            public static XName Item => _ns + "SitecoreItem";
            
            private static readonly XNamespace _ns = "http://schemas.microsoft.com/developer/msbuild/2003";
        }

        private readonly ILogger<TdsFileScanner> _logger;
        private readonly IFilePathFilter[] _filePathFilters;


        public TdsFileScanner(
            ILogger<TdsFileScanner> logger,
            IEnumerable<IFilePathFilter> filePathFilters)
        {
            _logger = logger;
            _filePathFilters = filePathFilters.ToArray();
        }
        
        public async IAsyncEnumerable<FileSet> FindFilesInPath(string root, string path)
        {
            _logger.LogInformation($"Scanning pattern {path}");
            var projectFiles = Glob.Files(root, path)
                                   .Select(path => Path.Combine(root, path))
                                   .ToList();
            _logger.LogInformation($"Pattern {path} found {projectFiles.Count} projects");
            
            foreach (var projectFilePath in projectFiles)
            {
                var fileSet = ReadTdsProject(projectFilePath);
                if (fileSet != null)
                {
                    yield return await fileSet;
                }
            }
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

                using var xmlReader = XmlReader.Create(projectFilePath, new XmlReaderSettings{Async = true});
                var xml = await XDocument.LoadAsync(xmlReader, LoadOptions.None, CancellationToken.None);
                var properties = ToDictionarySafe(xml.Root
                                 .Elements(TagNames.PropertyGroup)
                                 .Where(e => !e.HasAttributes)
                                 .Elements()
                                 .Select(e => KeyValuePair.Create(e.Name.LocalName, e.Value.Trim())));

                // If the project does not have codegen enabled, skip it.
                if (!properties.TryGetValue(ElementNames.EnableGeneration, out string enableCodegenString)
                    || !bool.TryParse(enableCodegenString, out bool enableCodegen)
                    || !enableCodegen)
                {
                    _logger.LogWarning($"{projectName} has not enabled source generation and will be skipped.");
                    return null;
                }

                var files = xml.Root
                               .Elements(TagNames.ItemGroup)
                               .Elements(TagNames.Item)
                               .Select(e => e.Attribute("Include")?.Value)
                               .Where(x => !string.IsNullOrEmpty(x))
                               .Select(DecodeFile)
                               .Select(x => Path.Combine(projectFolder, x))
                               .Where(EnsureItemFileExists)
                               .Where(f => _filePathFilters.All(filter => filter.Accept(f)))
                               .ToImmutableList();

                return new FileSet
                {
                    Files = files,
                    Id = properties[ElementNames.ProjectId],
                    Name = properties[ElementNames.ProjectName],
                    ItemPath = projectFolder,
                    ModelPath = Path.GetFullPath(Path.Combine(projectFolder, properties[ElementNames.ProjectSourcePath]))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not parse file {projectName}");
                return null;
            }
        }

        private IDictionary<string,string> ToDictionarySafe(IEnumerable<KeyValuePair<string, string>> properties)
        {
            Dictionary<string, string> Aggregator(Dictionary<string, string> d, KeyValuePair<string, string> kvp)
            {
                if (d.ContainsKey(kvp.Key))
                {
                    var currentValue = d[kvp.Key];
                    if (string.IsNullOrEmpty(currentValue))
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

        private string DecodeFile(string s)
        {
            return Regex.Replace(s, "%([0-9a-fA-F]{2})", match => DecodeValue(match.Groups[1].Value));
        }

        private string DecodeValue(string value)
        {
            return ((char) byte.Parse(value, NumberStyles.HexNumber)).ToString();
        }

        private bool EnsureItemFileExists(string itemFilePath)
        {
            if (File.Exists(itemFilePath))
            {
                return true;
            }

            _logger.LogWarning($"Item file {itemFilePath} does not exist.");
            return false;
        }
    }
}