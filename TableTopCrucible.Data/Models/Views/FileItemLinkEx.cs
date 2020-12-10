using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Data.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.Models.Views
{
    public struct  FileItemLinkEx
    {
        public FileItemLinkEx(FileItemLink link, FileInfoEx? thumbnail)
        {
            Link = link;
            Thumbnail = thumbnail;
        }

        public Version Version => Link.Version;
        public ItemId ItemId => Link.ItemId;
        public FileInfoHashKey FileKey => Link.FileKey;
        public FileItemLinkId Id => Link.Id;
        public FileItemLink Link { get; }
        public FileInfoEx? Thumbnail { get; }
    }
}
