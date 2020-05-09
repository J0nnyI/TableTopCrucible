using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TableTopCrucible.Domain.ValueTypes;
using TableTopCrucible.Domain.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public interface IItemChangeset : IEntityChangeset<Item, ItemId>
    {
        Thumbnail? Thumbnail { get; set; }
        ItemName Name { get; set; }
        IEnumerable<Tag> Tags { get; set; }
    }
    public class ItemChangeset : EntityChangesetBase<Item, ItemId>, IItemChangeset
    {
        private ItemName _name;
        public ItemName Name
        {
            get => this.getStructValue(_name, Origin?.Name);
            set => this.setStructValue(value, ref _name, Origin?.Name);
        }
        private IEnumerable<Tag> _tags;
        public IEnumerable<Tag> Tags
        {
            get => this.getValue(_tags, Origin?.Tags);
            set => this.setValue(value, ref _tags, Origin?.Tags);
        }
        private Thumbnail? _thumbnail;
        public Thumbnail? Thumbnail
        {
            get => this.getValue(_thumbnail, Origin?.Thumbnail);
            set => this.setValue(value, ref _thumbnail, Origin?.Thumbnail);
        }
        public ItemChangeset(Item? origin = null) : base(origin)
        {
        }

        public override Item Apply()
            => new Item(this.Origin.Value, this.Name, this.Tags,this.Thumbnail);
        public override Item ToEntity()
            => new Item(this.Name, this.Tags,this.Thumbnail);
        public override IEnumerable<string> GetErrors() => throw new NotImplementedException();

        public override string ToString()
            => "Changeset: " + Name;
    }
}
