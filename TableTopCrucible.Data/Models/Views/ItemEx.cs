
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

using Version = TableTopCrucible.Domain.Models.ValueTypes.Version;

namespace TableTopCrucible.Data.Models.Views
{
    public struct ItemEx
    {
        public ItemEx(Item item, IEnumerable<VersionedFile> files)
        {
            SourceItem = item;
            FileVersions = files;
        }

        public Item SourceItem { get; }
        public ItemName Name => SourceItem.Name;
        public ItemId ItemId => this.SourceItem.Id;
        public IEnumerable<Tag> Tags => SourceItem.Tags;
        public int FileCount => FileVersions
            .SelectMany(files => files.Files)
            .Count();
        public VersionedFile LatestVersionedFile =>
            FileVersions
                .OrderByDescending(x => x.Link.Version)
                .FirstOrDefault();
        public IEnumerable<DirectorySetup> DirectorySetups
            => FileVersions
            .OrderByDescending(x => x.Link.Version)
            .SelectMany(x => x.Files)
            .Select(x => x.DirectorySetup)
            .Distinct();

        public FileInfoEx? LatestFile =>
            LatestVersionedFile
            .Files
            .Select(x => x as FileInfoEx?)
            .FirstOrDefault(file => file?.IsFileAccessible == true);
        public Version? LatestVersion =>
            FileVersions.Any() ? (Version?)FileVersions.Max(x => x.Link.Version) : null;
        public IEnumerable<DirectorySetup> Directories =>
            FileVersions
            .SelectMany(x => x.Files)
            .Select(x => x.DirectorySetup)
            .Distinct();
        public IEnumerable<Version> Versions =>
            FileVersions
            .Select(x => x.Link.Version);
        public IEnumerable<VersionedFile> FileVersions { get; }
        public FileInfoEx? LatestThumbnail => this.LatestVersionedFile.Thumbnail;
        public IEnumerable<FileInfoHashKey> Thumbnails =>
            this.FileVersions
            .Select(file => file.Thumbnail)
            .Where(thumbnail => thumbnail.HasValue)
            .Cast<FileInfoHashKey>();

        public VersionedFile this[Version version]
        {
            get => this.FileVersions.FirstOrDefault(x => x.Version == version);
        }
    }
}
