using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Logging;
using ModelGenerator.Framework.FileParsing;

namespace ModelGenerator.Framework.ItemModelling
{
    public class Database : IDatabase
    {
        private readonly IImmutableDictionary<Guid, IImmutableList<Item>> _children;
        private readonly IImmutableDictionary<Guid, Item> _items;
        private readonly IImmutableDictionary<Guid, ItemSet> _itemSetsByItemId;
        private readonly IImmutableDictionary<string, ItemSet> _itemSetsByName;

        public Database(ILogger<DatabaseFactory> logger, IEnumerable<ItemSet> itemSets)
        {
            var items = new Dictionary<Guid, Item>();
            var children = new Dictionary<Guid, List<Item>>();
            var itemSetsByItemId = new Dictionary<Guid, ItemSet>();
            var itemSetsByName = new Dictionary<string, ItemSet>(StringComparer.OrdinalIgnoreCase);

            // TODO: Well-known items
            var root = new Item(
                ImmutableDictionary<HintTypes, string>.Empty,
                Guid.Parse("{11111111-1111-1111-1111-111111111111}"),
                "sitecore",
                Guid.Empty,
                "/sitecore",
                string.Empty,
                "Well known",
                ImmutableList<Field>.Empty,
                Guid.Empty,
                string.Empty,
                ImmutableList<LanguageVersion>.Empty
            );
            items.Add(root.Id, root);

            foreach (var itemSet in itemSets)
            {
                itemSetsByName.Add(itemSet.Name, itemSet);
                foreach (var item in itemSet.Items.Values)
                {
                    if (items.ContainsKey(item.Id))
                    {
                        logger.LogWarning($"Item {item.Path} duplicated between {itemSet.Name} and {itemSetsByItemId[item.Id].Name}.");
                        continue;
                    }

                    items.Add(item.Id, item);
                    itemSetsByItemId.Add(item.Id, itemSet);

                    if (!children.ContainsKey(item.Parent))
                    {
                        children.Add(item.Parent, new List<Item>());
                    }

                    children[item.Parent].Add(item);
                }
            }

            _items = items.ToImmutableDictionary();
            _children = children.ToImmutableDictionary(k => k.Key, v => (IImmutableList<Item>)v.Value.ToImmutableList());
            _itemSetsByName = itemSetsByName.ToImmutableDictionary();
            _itemSetsByItemId = itemSetsByItemId.ToImmutableDictionary();
        }

        public IImmutableList<Item> GetChildren(Guid itemId)
        {
            return _children.ContainsKey(itemId)
                ? _children[itemId]
                : ImmutableList<Item>.Empty;
        }

        public Item? GetItem(Guid id)
        {
            return _items.ContainsKey(id)
                ? _items[id]
                : null;
        }

        public ItemSet? GetItemSet(string name)
        {
            return _itemSetsByName.ContainsKey(name)
                ? _itemSetsByName[name]
                : null;
        }

        public ItemSet? GetItemSetForItem(Guid itemId)
        {
            return _itemSetsByItemId.ContainsKey(itemId)
                ? _itemSetsByItemId[itemId]
                : null;
        }

        public IEnumerable<Item> GetItemsWhere(Func<Item, bool> predicate)
        {
            return _items.Values.Where(predicate);
        }
    }
}