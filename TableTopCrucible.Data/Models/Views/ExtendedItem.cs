using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TableTopCrucible.Core.Utilities;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;

namespace TableTopCrucible.Domain.Models.Views
{
    public struct ExtendedItem
    {
        public ExtendedItem(Item item, ExtendedFileInfo? files)
        {
            this.Item = item;
            this.Files = files?.AsArray();
        }
        public ExtendedItem(Item item, IEnumerable<ExtendedFileInfo> files)
        {
            this.Item = item;
            this.Files = files;
        }

        public Item Item { get; }
        public ItemName Name => Item.Name;
        public Thumbnail? Thumbnail => Item.Thumbnail;
        public IEnumerable<Tag> Tags => Item.Tags;
        public ExtendedFileInfo? LatestFile => Files.FirstOrDefault();
        public IEnumerable<ExtendedFileInfo> Files { get; }
    }
}
