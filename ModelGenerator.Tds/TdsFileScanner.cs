using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
        

        public TdsFileScanner(ILogger<TdsFileScanner> logger)
        {
            _logger = logger;
        }
        
        public IEnumerable<FileSet> FindFilesInPath(string root, string path)
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
                    yield return fileSet;
                }
            }
        }

        private FileSet ReadTdsProject(string projectFilePath)
        {
            var projectName = Path.GetFileNameWithoutExtension(projectFilePath);
            _logger.LogInformation($"Reading {projectName}");
            try
            {
                var projectFolder = Path.GetDirectoryName(projectFilePath);
                if (projectFolder == null)
                {
                    throw new InvalidOperationException($"Could not resolve path from project {projectFolder}");
                }

                var xml = XDocument.Load(projectFilePath);
                var properties = xml.Root
                                 .Elements(TagNames.PropertyGroup)
                                 .Where(e => !e.HasAttributes)
                                 .Elements()
                                 .ToDictionary(e => e.Name.LocalName, e => e.Value);

                // If the project does not have codegen enabled, skip it.
                if (!properties.TryGetValue(ElementNames.EnableGeneration, out string enableCodegenString)
                    || !bool.TryParse(enableCodegenString, out bool enableCodegen)
                    || !enableCodegen)
                {
                    _logger.LogInformation($"{projectName} has not enabled source generation.");
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