using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;

using TableTopCrucible.Core.Models.ValueTypes;
using TableTopCrucible.Data.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using TableTopCrucible.WPF.Helper;

using SysFileInfo = System.IO.FileInfo;

namespace TableTopCrucible.Data.Models.Sources
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
                Path = origin.Value.Path;
                CreationTime = origin.Value.FileCreationTime;
                FileHash = origin.Value.FileHash;
                LastWriteTime = origin.Value.LastWriteTime;
                DirectorySetupId = origin.Value.DirectorySetupId;
                IsAccessible = origin.Value.IsAccessible;
                FileSize = origin.Value.FileSize;
            }
        }
        public FileInfoChangeset(DirectorySetup directorySetup, SysFileInfo fileInfo, FileHash hash) : this(null)
        {
            SetSysFileInfo(directorySetup, fileInfo);
            FileHash = hash;
        }
        public void SetSysFileInfo(DirectorySetup directorySetup, SysFileInfo fileInfo)
        {
            DirectorySetupId = directorySetup.Id;
            Path = fileInfo != null ? directorySetup.Path.MakeUnescapedRelativeUri(fileInfo?.FullName) : null;
            CreationTime = fileInfo?.CreationTime ?? default;
            LastWriteTime = fileInfo?.LastWriteTime ?? default;
            FileSize = fileInfo?.Length ?? default;
            IsAccessible = fileInfo != null;
        }
        public override FileInfo Apply()
            => new FileInfo(Origin.Value, Path, CreationTime, FileHash, LastWriteTime, DirectorySetupId, FileSize, IsAccessible);
        public override IEnumerable<string> GetErrors() => throw new NotImplementedException();
        public override FileInfo ToEntity()
            => new FileInfo(Path, CreationTime, FileHash, LastWriteTime, DirectorySetupId, FileSize, IsAccessible);
    }
}
