using System.Collections.Generic;
using ModelGenerator.Framework.FileParsing;

namespace ModelGenerator.Framework.ItemModelling
{
    public interface IDatabaseFactory
    {
        IDatabase CreateDatabase(IEnumerable<ItemSet> itemSets);
    }
}