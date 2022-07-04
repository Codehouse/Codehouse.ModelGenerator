using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.FileScanning;
using ModelGenerator.Framework.Progress;
using ModelGenerator.Scs.FileParsing;

namespace ModelGenerator.Scs.FileScanning
{
    public class ScsFileScanner : IFileScanner
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly ILogger<ScsFileScanner> _log;
        private readonly ScsSettings _settings;

        public ScsFileScanner(ILogger<ScsFileScanner> log, ScsSettings settings)
        {
            _log = log;
            _settings = settings;
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<Module?> ReadModule(OwnedScopedRagBuilder<string> ragBuilder, string path)
        {
            try
            {
                var moduleFileContent = await File.ReadAllTextAsync(path);
                var module = JsonSerializer.Deserialize<Module>(moduleFileContent, _jsonSerializerOptions);
                if (module == null)
                {
                    ragBuilder.AddFail("SCS module {could not be read.");
                }

                return module;
            }
            catch (Exception ex)
            {
                ragBuilder.AddFail("Error while parsing SCS module", ex);
                return null;
            }
        }

        public async Task<FileSet?> ScanSourceAsync(RagBuilder<string> ragBuilder, string path)
        {
            using var tracker = ragBuilder.CreateScope(Path.GetFileName(path));

            var module = await ReadModule(tracker, path);
            if (module == null)
            {
                return null;
            }

            var moduleFolder = Path.GetDirectoryName(path);
            if (moduleFolder == null)
            {
                ragBuilder.AddFail(new RagStatus<string>($"Could not determine module folder for SCS module {Path.GetFileName(path)}."));
                return null;
            }

            var items = module.Items.Includes
                              .SelectMany(i => ScanItemSource(i, moduleFolder, tracker))
                              .ToImmutableList();

            if (tracker.CanPass)
            {
                tracker.AddPass();
            }

            return new FileSet(items,
                module.Namespace,
                Path.Combine(moduleFolder, _settings.ItemFolder),
                Path.GetFullPath(Path.Combine(moduleFolder, _settings.ModelFolder)),
                module.Namespace,
                _settings.BaseNamespace + "." + module.Namespace,
                ImmutableArray<string>.Empty);
        }

        private IEnumerable<ItemFile> ScanItemSource(ItemSource itemSource, string moduleFolder, ScopedRagBuilder<string> ragBuilder)
        {
            var itemSourceFolder = Path.Combine(moduleFolder, _settings.ItemFolder, itemSource.Name);
            _log.LogInformation("Scanning item source folder ({folder}) for items.", itemSourceFolder);
            if (!Directory.Exists(itemSourceFolder))
            {
                _log.LogError("Item source folder ({folder}) does not exist.", itemSourceFolder);
                ragBuilder.AddFail($"Item source folder does not exist ({itemSourceFolder}).");
            }
            else
            {
                var files = Directory.GetFiles(itemSourceFolder, "*.yml", SearchOption.AllDirectories);
                _log.LogInformation("Found {count} files", files.Length);

                if (!files.Any())
                {
                    ragBuilder.AddWarn($"Item source {itemSource.Name} contains no files.");
                }

                foreach (var file in files)
                {
                    yield return new ItemFile(file, new Dictionary<string, string>());
                }
            }
        }
    }
}