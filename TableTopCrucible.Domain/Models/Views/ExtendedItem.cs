using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TableTopCrucible.Domain.Models.Sources;

using Version = TableTopCrucible.Domain.Models.ValueTypes.Version;

namespace TableTopCrucible.Domain.Models.Views
{
    public struct ExtendedItem
    {
        public ExtendedItem(Item item, ExtendedFileInfo files)
        {
            this.Item = item;
            this.LatestFile = files;
        }

        public Item Item { get; }
        public ExtendedFileInfo LatestFile { get; }
    }
}
