
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TableTopCrucible.Core.Utilities;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.Views;

namespace TableTopCrucible.Data.Models.Views
{
    public struct ExtendedItem
    {
        public ExtendedItem(Item item, IEnumerable<VersionedFile> files)
        {
            Item = item;
            Files = files;
        }

        public Item Item { get; }
        public ItemName Name => Item.Name;
        public Thumbnail? Thumbnail => Item.Thumbnail;
        public IEnumerable<Tag> Tags => Item.Tags;
        public int FileCount => Files
            .SelectMany(files => files.Files)
            .Count();
        public ExtendedFileInfo? LatestFile => 
            Files.OrderByDescending(x=>x.Link.Version)
                .FirstOrDefault()
                .Files
                .Select(x=>x as ExtendedFileInfo?)
                .FirstOrDefault(file=>file?.IsFileAccessible==true);
        public Version? LatestVersion =>
            Files.Max(x => x.Link.Version);
        public IEnumerable<DirectorySetup> Directories =>
            Files
            .SelectMany(x => x.Files)
            .Select(x => x.DirectorySetup)
            .Distinct();
        public IEnumerable<VersionedFile> Files { get; }
    }
}
