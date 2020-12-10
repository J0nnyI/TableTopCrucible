using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Data.Models.Sources;

namespace TableTopCrucible.Data.Models.Views
{
    public struct ThumbnailItem
    {
        public ThumbnailItem(Item item,FileItemLink link, FileInfoEx fileInfo)
        {
            Item = item;
            Link = link;
            FileInfo = fileInfo;
        }

        public Item Item { get; }
        public FileItemLink Link { get; }
        public FileInfoEx FileInfo { get; }

    }
}
