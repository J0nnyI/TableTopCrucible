using TableTopCrucible.Domain.Models.Sources;

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
