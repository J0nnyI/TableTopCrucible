using System;
using System.Collections.Generic;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

using Version = TableTopCrucible.Domain.Models.ValueTypes.Version;

namespace TableTopCrucible.Domain.Models.Sources
{
    public class FileItemLinkChangeset : IEntityChangeset<FileItemLink, FileItemLinkId>
    {
        public ItemId ItemId { get; set; }

        public FileInfoHashKey FileKey { get; set; }

        public Version Version { get; set; }
        public FileItemLink? Origin { get; set; }
        public FileInfoHashKey? ThumbnailKey { get; set; }
        public FileItemLinkChangeset(FileItemLink? origin = null) 
        {
            this.Origin = origin;
            if (origin.HasValue) {
                this.ItemId = origin.Value.ItemId;
                this.Version = origin.Value.Version;
                this.FileKey = origin.Value.FileKey;
                this.ThumbnailKey = origin.Value.ThumbnailKey;
            }
        }
        public FileItemLink Apply() => new FileItemLink(Origin.Value, this.ItemId, this.FileKey, this.ThumbnailKey, this.Version);
        public IEnumerable<string> GetErrors() => throw new NotImplementedException();
        public FileItemLink ToEntity() => new FileItemLink(this.ItemId, this.FileKey, this.ThumbnailKey, this.Version);
    }
}
