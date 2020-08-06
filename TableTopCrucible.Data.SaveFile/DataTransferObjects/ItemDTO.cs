using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Media.Animation;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.SaveFile.DataTransferObjects
{
    [DataContract]
    public class ItemDTO : EntityDtoBase
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public IEnumerable<string> Tags { get; set; }
        [DataMember]
        public string Thumbnail { get; set; }

        public ItemDTO()
        {

        }

        public ItemDTO(Item source) : base(source)
        {
            this.Name = (string)source.Name;
            this.Tags = source.Tags.Select(x => (string)x).ToArray();
        }


        public Item ToEntity()
            => new Item(
                (ItemId)this.Id,
                (ItemName)this.Name,
                this.Tags?.Select(x => (Tag)x)?.ToArray(),
                this.Created,
                this.LastChange);
    }
}
