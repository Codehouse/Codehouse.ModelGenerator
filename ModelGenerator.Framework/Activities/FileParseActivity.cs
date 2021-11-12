using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.FileParsing;
using ModelGenerator.Framework.FileScanning;
using ModelGenerator.Framework.Progress;

namespace ModelGenerator.Framework.Activities
{
    public class FileParseActivity : CollectionActivityBase<FileSet, ItemSet>
    {
        public override string Description => "Parsing item files";
        private readonly IFileParser _fileParser;

        private readonly ILogger<FileParseActivity> _logger;

        public FileParseActivity(ILogger<FileParseActivity> logger, IFileParser fileParser)
        {
            _logger = logger;
            _fileParser = fileParser;
        }

        protected override async Task<ItemSet?> ExecuteItemAsync(Job job, FileSet input)
        {
            try
            {
                _logger.LogDebug($"Found {input.Files.Count} files in {input.Name}");
                var tasks = input.Files
                                 .Select(f => Task.Run(() => _fileParser.ParseFile(input, f)));
                var items = (await Task.WhenAll(tasks))
                            .SelectMany(i => i)
                            .ToDictionary(i => i.Id, i => i);

                return new ItemSet
                {
                    Id = input.Id,
                    Name = input.Name,
                    Namespace = input.Namespace,
                    ItemPath = input.ItemPath,
                    ModelPath = input.ModelPath,
                    References = input.References,
                    Items = items.ToImmutableDictionary()
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not parse files in fileset {input.Name}");
                throw;
            }
        }
    }
}