using System;

using TableTopCrucible.Base.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

using Version = TableTopCrucible.Domain.Models.ValueTypes.Version;

namespace TableTopCrucible.Domain.Models.Sources
{
    public struct FileItemLink : IEntity<FileItemLinkId>
    {
        public FileItemLink(FileItemLink origin, ItemId itemId, FileInfoHashKey fileKey, Version version) : this(origin.Id, itemId, fileKey, version, DateTime.Now) { }
        public FileItemLink(ItemId itemId, FileInfoHashKey fileKey, Version version) : this((FileItemLinkId)Guid.NewGuid(), itemId, fileKey, version, DateTime.Now) { }

        public FileItemLink(FileItemLinkId id, ItemId itemId, FileInfoHashKey fileKey, Version version, DateTime created)
        {
            this.Id = id;
            this.ItemId = itemId;
            this.FileKey = fileKey;
            this.Version = version;
            this.Created = created;
            this.LastChange = DateTime.Now;
        }

        public FileItemLinkId Id { get; }
        public ItemId ItemId { get; }

        public FileInfoHashKey FileKey { get; }

        public Version Version { get; }

        public DateTime Created { get; }
        public DateTime LastChange { get; }
    }
}
