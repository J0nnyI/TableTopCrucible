using System;
using System.Runtime.Serialization;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Models.ValueTypes.IDs;

namespace TableTopCrucible.SaveFile.DataTransferObjects
{
    [DataContract]
    public abstract class EntityDtoBase
    {
        [DataMember]
        public DateTime Created { get; set; }
        [DataMember]
        public DateTime LastChange { get; set; }
        [DataMember]
        public Guid Id { get; set; }
        public EntityDtoBase(IEntity source)
        {
            this.Created = source.Created;
            this.LastChange = source.LastChange;
            this.Id = source.Id;
        }
        public EntityDtoBase()
        {

        }
    }
}
