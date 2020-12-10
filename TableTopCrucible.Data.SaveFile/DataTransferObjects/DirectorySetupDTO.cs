using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

using TableTopCrucible.Data.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.SaveFile.DataTransferObjects
{
    [DataContract]
    public class DirectorySetupDTO : EntityDtoBase
    {
        [DataMember]
        public Uri Path { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        public DirectorySetupDTO(DirectorySetup source) : base(source)
        {
            this.Path = source.Path;
            this.Name = (string)source.Name;
            this.Description = (string)source.Description;
        }
        public DirectorySetupDTO()
        {

        }

        public DirectorySetup ToEntity()
            => new DirectorySetup(
                Path,
                (DirectorySetupName)Name,
                (Description)Description,
                (DirectorySetupId)Id,
                Created,
                LastChange);
    }
}
