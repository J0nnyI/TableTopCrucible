using System;
using System.Collections.Generic;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.Models.Sources
{
    public struct Item : IEntity<ItemId>
    {
        public ItemId Id { get; }
        public ItemName Name { get; }
        public IEnumerable<Tag> Tags { get; }
        public Guid Identity { get; }

        public DateTime Created { get; }
        public DateTime LastChange { get; }

        public Item(ItemName name, IEnumerable<Tag> tags = null)
            : this((ItemId)Guid.NewGuid(), name, tags, DateTime.Now) { }
        public Item(Item origin, ItemName name, IEnumerable<Tag> tags)
            : this(origin.Id, name, tags, origin.Created) { }

        public Item(ItemId id, ItemName name, IEnumerable<Tag> tags, DateTime created, DateTime? lastChange = null)
        {
            Id = id;
            Name = name;
            Tags = tags ?? throw new ArgumentNullException(nameof(tags));
            Created = created;
            LastChange = lastChange ?? DateTime.Now;
            Identity = Guid.NewGuid();
        }

        public override string ToString() => $"Tile {Id} ({Name})";
        public override bool Equals(object obj) => obj is Item item && this == item;
        public override int GetHashCode() => HashCode.Combine(Identity);

        public static bool operator ==(Item itemA, Item itemB)
            => itemA.Identity == itemB.Identity;
        public static bool operator !=(Item itemA, Item itemB)
            => itemA.Identity != itemB.Identity;
    }
}
