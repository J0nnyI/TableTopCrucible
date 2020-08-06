using System;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

using Version = TableTopCrucible.Domain.Models.ValueTypes.Version;

namespace TableTopCrucible.Domain.Models.Sources
{
    public struct FileItemLink : IEntity<FileItemLinkId>
    {
        public FileItemLink(
            FileItemLink origin,
            ItemId itemId,
            FileInfoHashKey fileKey,
            FileInfoHashKey? thumbnailKey,
            Version version)
            : this(
            origin.Id,
            itemId == default ? origin.ItemId : itemId,
            fileKey == default ? origin.FileKey : fileKey,
            thumbnailKey == default ? origin.ThumbnailKey : thumbnailKey,
            version == default ? origin.Version : version,
            DateTime.Now)
        { }


        public FileItemLink(
            ItemId itemId,
            FileInfoHashKey fileKey,
            FileInfoHashKey? thumbnailKey,
            Version version)
            : this(
            (FileItemLinkId)Guid.NewGuid(),
            itemId,
            fileKey,
            thumbnailKey,
            version,
            DateTime.Now)
        { }


        public FileItemLink(
            FileItemLinkId id,
            ItemId itemId,
            FileInfoHashKey fileKey,
            FileInfoHashKey? thumbnailKey,
            Version version,
            DateTime created,
            DateTime? lastChange = null)
        {
            this.Id = id;
            this.ItemId = itemId;
            this.FileKey = fileKey;
            this.Version = version;
            this.Created = created;
            this.ThumbnailKey = thumbnailKey;
            this.LastChange = lastChange ?? DateTime.Now;
        }

        public FileItemLinkId Id { get; }
        public ItemId ItemId { get; }

        public FileInfoHashKey FileKey { get; }
        public FileInfoHashKey? ThumbnailKey { get; }
        public Version Version { get; }

        public DateTime Created { get; }
        public DateTime LastChange { get; }
    }
}
