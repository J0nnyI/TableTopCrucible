using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

using TableTopCrucible.Data.SaveFile.DataTransferObjects;

namespace TableTopCrucible.Data.SaveFile.Tests.DataTransferObjects
{
    [DataContract]
    public class MasterDTO
    {
        [DataMember]
        public IEnumerable<FileInfoDTO> ModelFiles { get; set; }
        [DataMember]
        public IEnumerable<FileInfoDTO> ImageFiles { get; set; }
        [DataMember]
        public IEnumerable<ItemDTO> Items { get; set; }
        [DataMember]
        public IEnumerable<FileItemLinkDTO> FileItemLinks{ get; set; }
        [DataMember]
        public IEnumerable<DirectorySetupDTO> Directories { get; set; }
    }
}
