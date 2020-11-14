
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Data.Models.ValueTypes;
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
        public string RealtiveThumbnailPath => LatestFile?.DirectorySetup.ThumbnailUri.ToString().TrimStart('\\');
        public string RootPath => RootUri.LocalPath;
        public Uri RootUri => LatestFile?.DirectorySetup.Path;
        public int FileCount => FileVersions
            .SelectMany(files => files.Files)
            .Count();
        public VersionedFile? LatestVersionedFile =>
            FileVersions
                .OrderByDescending(x => x.Link.Version)
                .Select(file => file as VersionedFile?)
                .FirstOrDefault();
        public string GenerateThumbnailFilename()
            => @$"{Name}_{DateTime.Now:yyyyMMdd_HHmmssss}.png";
        public string GenerateRelativeThumbnailPath(string filename = null)
            => Path.Combine(RealtiveThumbnailPath, filename ?? GenerateThumbnailFilename());
        public string GenerateAbsoluteThumbnailPath(string filename = null)
            => Path.Combine(RootPath, GenerateRelativeThumbnailPath(filename));
        public string LatestFilePath => LatestVersionedFile?.File.AbsolutePath;
        public Uri LatestFileUri => LatestVersionedFile?.File.AbsoluteUri;
        public IEnumerable<DirectorySetup> DirectorySetups
            => FileVersions
            .OrderByDescending(x => x.Link.Version)
            .SelectMany(x => x.Files)
            .Select(x => x.DirectorySetup)
            .Distinct();

        public FileInfoEx? LatestFile =>
            LatestVersionedFile
            ?.Files
            ?.Select(x => x as FileInfoEx?)
            ?.FirstOrDefault(file => file?.IsFileAccessible == true);
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
        public bool HasFiles => FilePaths.Any();
        public IEnumerable<string> FilePaths => this.FileVersions.SelectMany(file => file.FilePaths);
        public FileInfoEx? LatestThumbnail => this.LatestVersionedFile?.Thumbnail;
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
