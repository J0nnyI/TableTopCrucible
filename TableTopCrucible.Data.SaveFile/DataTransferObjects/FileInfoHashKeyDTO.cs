using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Unicode;

using TableTopCrucible.Domain.Models.ValueTypes;

namespace TableTopCrucible.Data.SaveFile.DataTransferObjects
{
    [DataContract]
    public class FileInfoHashKeyDTO
    {
        [DataMember]
        public byte[] Hash { get; set; }
        [DataMember]
        public long FileSize { get; set; }
        public FileInfoHashKeyDTO()
        {

        }
        public FileInfoHashKeyDTO(FileInfoHashKey origin)
        {
            this.Hash = origin.FileHash.Data;
            this.FileSize = origin.FileSize;
        }
        public FileInfoHashKey ToEntity()
            => new FileInfoHashKey(new FileHash(Hash), FileSize);
    }
}
