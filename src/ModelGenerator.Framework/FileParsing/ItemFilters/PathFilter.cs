using System;
using System.Linq;
using ModelGenerator.Framework.Progress;

namespace ModelGenerator.Framework.FileParsing.ItemFilters
{
    public class PathFilter : IItemFilter
    {
        private readonly PathFilterSettings _settings;

        public PathFilter(PathFilterSettings settings)
        {
            _settings = settings;
        }

        public bool Accept(ScopedRagBuilder<string> tracker, Item item)
        {
            return _settings.Exclude.All(excludedPath => !item.Path.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase));
        }
    }
}