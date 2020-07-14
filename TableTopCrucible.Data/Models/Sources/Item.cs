using System;
using System.Collections.Generic;

using TableTopCrucible.Core.Models.Sources;
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
        public Guid Identity { get; }
        public FileInfoHashKey? File { get; }

        public DateTime Created { get; }
        public DateTime LastChange { get; }

        public Item(ItemName name, IEnumerable<Tag> tags = null, FileInfoHashKey? file = null, Thumbnail? thumbnail = null)
            : this((ItemId)Guid.NewGuid(), name, tags, file, thumbnail, DateTime.Now) { }
        public Item(Item origin, ItemName name, IEnumerable<Tag> tags, FileInfoHashKey? file, Thumbnail? thumbnail)
            : this(origin.Id, name, tags, file, thumbnail, origin.Created) { }

        public Item(ItemId id, ItemName name, IEnumerable<Tag> tags, FileInfoHashKey? file, Thumbnail? thumbnail, DateTime created, DateTime? lastChange = null)
        {
            this.Id = id;
            this.Name = name;
            this.Thumbnail = thumbnail;
            this.Tags = tags ?? throw new ArgumentNullException(nameof(tags));
            this.File = file;
            this.Created = created;
            this.LastChange = lastChange ?? DateTime.Now;
            this.Identity = Guid.NewGuid();
        }

        public override string ToString() => $"Tile {Id} ({Name})";
        public override bool Equals(object obj) => obj is Item item && this == item;
        public override int GetHashCode() => HashCode.Combine(this.Identity);

        public static bool operator ==(Item itemA, Item itemB)
            => itemA.Identity == itemB.Identity;
        public static bool operator !=(Item itemA, Item itemB)
            => itemA.Identity != itemB.Identity;
    }
}
