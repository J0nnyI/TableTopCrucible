using System;
using System.Collections.Generic;

using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public struct Item : IEntity<ItemId>
    {
        public ItemId Id { get; }
        public ItemName Name { get; }
        public Thumbnail? Thumbnail { get; }
        public IEnumerable<Tag> Tags { get; }

        public DateTime Created { get; }
        public DateTime LastChange { get; }

        public Item(ItemName name, IEnumerable<Tag> tags = null, Thumbnail? thumbnail = null)
        {
            this.Id = (ItemId)Guid.NewGuid();
            this.Name = name;
            this.Tags = tags;
            this.Created = DateTime.Now;
            this.LastChange = DateTime.Now;
            this.Thumbnail = thumbnail;
        }
        public Item(Item origin, ItemName name, IEnumerable<Tag> tags, Thumbnail? thumbnail)
        {
            this.Id = origin.Id;
            this.Name = name;
            this.Tags = tags;
            this.Created = origin.Created;
            this.LastChange = origin.LastChange;
            this.Thumbnail = thumbnail;
        }
        public override string ToString() => $"Tile {Id} ({Name})";
    }
}
