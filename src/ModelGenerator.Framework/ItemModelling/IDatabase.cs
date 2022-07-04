using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ModelGenerator.Framework.FileParsing;

namespace ModelGenerator.Framework.ItemModelling
{
    public interface IDatabase
    {
        IImmutableList<Item> GetChildren(Guid itemId);

        Item? GetItem(Guid id);

        ItemSet? GetItemSet(string name);

        ItemSet? GetItemSetForItem(Guid itemId);

        IEnumerable<Item> GetItemsWhere(Func<Item, bool> predicate);
    }
}