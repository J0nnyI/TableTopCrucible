using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public struct FileInfo : IEntity<FileInfoId>
    {
        public FileInfo(
            FileInfo origin, Uri path, DateTime creationTime, FileHash fileHash, DateTime lastWriteTime, DirectorySetupId directorySetupId,
            bool isAccessible, bool isNew)
            : this(path, creationTime, fileHash, lastWriteTime, directorySetupId, isAccessible, isNew, origin.Id, origin.Created)
        { }
        public FileInfo(
            Uri path, DateTime creationTime, FileHash fileHash, DateTime lastWriteTime, DirectorySetupId directorySetupId,
            bool isAccessible, bool isNew)
            : this(path, creationTime, fileHash, lastWriteTime, directorySetupId, isAccessible, isNew, (FileInfoId)Guid.NewGuid(), DateTime.Now)
        { }
        private FileInfo(
            Uri path, DateTime creationTime, FileHash fileHash, DateTime lastWriteTime, DirectorySetupId directorySetupId,
            bool isAccessible, bool isNew, FileInfoId id, DateTime created)
        {
            this.Path = path;
            this.CreationTime = creationTime;
            this.FileHash = fileHash;
            this.LastWriteTime = lastWriteTime;
            this.DirectorySetupId = directorySetupId;
            this.isAccessible = isAccessible;
            this.IsNew = isNew;
            this.Id = id;
            this.Created = created;
            this.LastChange = DateTime.Now;
        }

        public Uri Path { get; }
        public DateTime CreationTime { get; }
        public FileHash FileHash { get; }
        public DateTime LastWriteTime { get; }
        public DirectorySetupId DirectorySetupId { get; }
        public bool isAccessible { get; }
        public bool IsNew { get; }

        public FileInfoId Id { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }
    }
}
