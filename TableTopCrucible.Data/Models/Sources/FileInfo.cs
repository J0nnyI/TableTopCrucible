using System;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.Models.Sources
{
    public struct FileInfo : IEntity<FileInfoId>
    {
        public FileInfo(
            FileInfo origin, Uri path, DateTime creationTime, FileHash? fileHash, DateTime lastWriteTime, DirectorySetupId directorySetupId, long fileSize,
            bool isAccessible)
            : this(path, creationTime, fileHash, lastWriteTime, directorySetupId, fileSize, isAccessible, origin.Id, origin.Created)
        { }
        public FileInfo(
            Uri path, DateTime creationTime, FileHash? fileHash, DateTime lastWriteTime, DirectorySetupId directorySetupId, long fileSize,
            bool isAccessible)
            : this(path, creationTime, fileHash, lastWriteTime, directorySetupId, fileSize, isAccessible, (FileInfoId)Guid.NewGuid(), DateTime.Now)
        { }
        public FileInfo(
            Uri path, DateTime creationTime, FileHash? fileHash, DateTime lastWriteTime, DirectorySetupId directorySetupId, long fileSize,
            bool isAccessible, FileInfoId id, DateTime created, DateTime? lastChange = null)
        {
            this.Path = path;
            this.FileCreationTime = creationTime;
            this.FileHash = fileHash;
            this.LastWriteTime = lastWriteTime;
            this.DirectorySetupId = directorySetupId;
            this.FileSize = fileSize;
            this.IsAccessible = isAccessible;
            this.Id = id;
            this.Created = created;
            this.LastChange = lastChange ?? DateTime.Now;
            this.Identity = Guid.NewGuid();
            this.HashKey = null;
            if (FileInfoHashKey.CanBuild(this))
                this.HashKey = new FileInfoHashKey(this);
        }

        public FileInfoHashKey? HashKey { get; }
        public Uri Path { get; }
        // the time when the file (not the model) was created
        public DateTime FileCreationTime { get; }
        public FileHash? FileHash { get; }
        public DateTime LastWriteTime { get; }
        public DirectorySetupId DirectorySetupId { get; }
        public bool IsAccessible { get; }
        // identifies this item in this specific state
        public long FileSize { get; }
        public Guid Identity { get; }

        public FileInfoId Id { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }

        public static bool operator ==(FileInfo fileA, FileInfo fileB)
            => fileA.Identity == fileB.Identity;
        public static bool operator !=(FileInfo fileA, FileInfo fileB)
            => fileA.Identity != fileB.Identity;

        public override bool Equals(object obj) => obj is FileInfo info && this == info;
        public override int GetHashCode() => HashCode.Combine(this.Identity);
    }
}
