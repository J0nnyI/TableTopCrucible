using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;

using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public class FileInfoChangeset : EntityChangesetBase<FileInfo, FileInfoId>, IEntityChangeset<FileInfo, FileInfoId>
    {
        [Reactive]
        public Uri Path { get; set; }

        [Reactive]
        public DateTime CreationTime { get; set; }

        [Reactive]
        public FileHash? FileHash { get; set; }

        [Reactive]
        public DateTime LastWriteTime { get; set; }

        [Reactive]
        public DirectorySetupId DirectorySetupId { get; set; }

        [Reactive]
        public bool IsAccessible { get; set; }

        [Reactive]
        public long FileSize { get; set; }



        public FileInfoChangeset(FileInfo? origin = null) : base(origin)
        {
            if (origin.HasValue)
            {
                this.Path = origin.Value.Path;
                this.CreationTime = origin.Value.CreationTime;
                this.FileHash = origin.Value.FileHash;
                this.LastWriteTime = origin.Value.LastWriteTime;
                this.DirectorySetupId = origin.Value.DirectorySetupId;
                this.IsAccessible = origin.Value.IsAccessible;
            }
        }

        public override FileInfo Apply()
            => new FileInfo(this.Origin.Value, Path, CreationTime, FileHash, LastWriteTime, DirectorySetupId, FileSize, IsAccessible);
        public override IEnumerable<string> GetErrors() => throw new NotImplementedException();
        public override FileInfo ToEntity()
            => new FileInfo(Path, CreationTime, FileHash, LastWriteTime, DirectorySetupId, FileSize, IsAccessible);
    }
}
