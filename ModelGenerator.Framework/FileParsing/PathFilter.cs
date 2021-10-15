using System;
using System.Linq;

namespace ModelGenerator.Framework.FileParsing
{
    public class PathFilter : IItemFilter
    {
        private readonly PathFilterSettings _settings;

        public PathFilter(PathFilterSettings settings)
        {
            _settings = settings;
        }
        
        public bool Accept(Item item)
        {
            return _settings.Exclude.All(excludedPath => !item.Path.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase));
        }
    }
}