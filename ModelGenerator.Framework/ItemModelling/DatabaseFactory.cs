using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.FileParsing;

namespace ModelGenerator.Framework.ItemModelling
{
    public class DatabaseFactory : IDatabaseFactory
    {
        private readonly ILogger<DatabaseFactory> _logger;

        public DatabaseFactory(ILogger<DatabaseFactory> logger)
        {
            _logger = logger;
        }
        
        public IDatabase CreateDatabase(IEnumerable<ItemSet> itemSets)
        {
            return new Database(_logger, itemSets);
        }
    }
}