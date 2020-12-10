using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

using TableTopCrucible.Data.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

using Version = TableTopCrucible.Domain.Models.ValueTypes.Version;

namespace TableTopCrucible.Data.SaveFile.DataTransferObjects
{
    [DataContract]
    public class FileItemLinkDTO : EntityDtoBase
    {
        [DataMember]
        public Guid ItemId { get; set; }

        [DataMember]
        public FileInfoHashKeyDTO FileKey { get; set; }

        [DataMember]
        public VersionDTO Version { get; set; }
        [DataMember]
        public FileInfoHashKeyDTO ThumbnailKey { get; set; }
        public FileItemLinkDTO()
        {

        }
        public FileItemLinkDTO(FileItemLink source) : base(source)
        {
            this.ItemId = (Guid)source.ItemId;
            this.FileKey = new FileInfoHashKeyDTO(source.FileKey);
            this.Version = new VersionDTO(source.Version);
            this.ThumbnailKey = source.ThumbnailKey.HasValue ? new FileInfoHashKeyDTO(source.ThumbnailKey.Value) : null;
        }

        public FileItemLink ToEntity()
            => new FileItemLink((FileItemLinkId)Id, (ItemId)ItemId, FileKey.ToEntity(), ThumbnailKey?.ToEntity(), Version.ToEntity(), Created, LastChange);
    }
}
