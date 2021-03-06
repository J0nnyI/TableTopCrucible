﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.SaveFile.DataTransferObjects
{
    [DataContract]
    public class FileInfoDTO : EntityDtoBase
    {
        [DataMember]
        public DateTime FileCreationTime { get; set; }
        [DataMember]
        public Uri Path { get; set; }
        [DataMember]
        public byte[] FileHash { get; set; }
        [DataMember]
        public DateTime LastWriteTime { get; set; }
        [DataMember]
        public Guid DirectorySetupId { get; set; }
        [DataMember]
        public bool IsAccessible { get; set; }
        [DataMember]
        public long FileSize { get; set; }
        public FileInfoDTO(FileInfo source) : base(source)
        {
            Path = source.Path;
            FileCreationTime = source.FileCreationTime;
            FileHash = source.FileHash?.Data;
            LastWriteTime = source.LastWriteTime;
            DirectorySetupId = (Guid)source.DirectorySetupId;
            IsAccessible = source.IsAccessible;
            FileSize = source.FileSize;

        }
        public FileInfoDTO()
        {

        }

        public FileInfo ToEntity()
            => new FileInfo(
                Path,
                FileCreationTime,
                FileHash != null ? new FileHash(FileHash) as FileHash? : null,
                LastWriteTime,
                (DirectorySetupId)DirectorySetupId,
                FileSize,
                IsAccessible,
                (FileInfoId)Id,
                Created,
                LastChange);
    }
}
