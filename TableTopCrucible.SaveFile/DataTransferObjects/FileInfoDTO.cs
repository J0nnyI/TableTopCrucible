using System;
using System.Collections.Generic;
using System.Text;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.SaveFile.DataTransferObjects
{
    public class FileInfoDTO: EntityDtoBase
    {
        public FileInfoHashKey? HashKey { get; set; }
        public DateTime CreationTime { get; set; }
        public Uri Path { get; set; }
        public FileHash? FileHash { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DirectorySetupId DirectorySetupId { get; set; }
        public bool IsAccessible { get; set; }
        public long FileSize { get; set; }
        public FileInfoDTO(FileInfo source) :base(source)
        {
            HashKey = source.HashKey;
            Path = source.Path;
            CreationTime = source.CreationTime;
            FileHash = source.FileHash;
            LastWriteTime = source.LastWriteTime;
            DirectorySetupId = source.DirectorySetupId;
            IsAccessible = source.IsAccessible;
            FileSize = source.FileSize;

        }
        public FileInfoDTO()
        {

        }

        public FileInfo ToEntity()
            => new FileInfo(Path, CreationTime, FileHash, LastWriteTime, DirectorySetupId, FileSize, IsAccessible, (FileInfoId)Id, Created, LastChange);
    }
}
