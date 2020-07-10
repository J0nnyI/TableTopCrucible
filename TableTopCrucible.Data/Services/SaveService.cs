using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TableTopCrucible.Core.Services;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;

namespace TableTopCrucible.Data.Services
{
    public interface ISaveService
    {
        void Load(string file);
    }

    public class SaveService : ISaveService
    {
        private readonly IItemService _itemService;
        private readonly IDirectoryDataService _directoryDataService;
        private readonly ISettingsService _settingsService;

        public SaveService(IItemService itemService, IDirectoryDataService directoryDataService, ISettingsService settingsService)
        {
            this._itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            this._directoryDataService = directoryDataService ?? throw new ArgumentNullException(nameof(directoryDataService));
            this._settingsService = settingsService;
        }

        public void Load(string file)
        {
            _devTestSetup();
        }


        private void _devTestSetup()
        {
            _addItem("test 1");
            this._itemService.Patch(_getTaggyItem("taggy item 1"));
            this._itemService.Patch(_getTaggyItem("taggy item 2"));
            _directoryDataService.Patch(new DirectorySetupChangeset()
            {
                Path = @"F:\tmp\Folder A",
                Name = @"Folder A"
            });

            _directoryDataService.Patch(new DirectorySetupChangeset()
            {
                Path = @"D:\__MANAGED_FILES__\DnD\Shelf\wallhalla-fantasy-stonework",
                Name = @"Test Folder"
            });
            _directoryDataService.Patch(new DirectorySetupChangeset()
            {
                Path = @"F:\tmp\Folder B",
                Name = @"Folder B"
            });
        }


        private void _addItem(string name)
            => _itemService.Patch(_getItem(name));
        private void _addItems(params string[] names)
            => _itemService.Patch(names.Select(name => _getItem(name)));
        private ItemChangeset _getItem(string name)
        {
            return new ItemChangeset()
            {
                Name = name,
                Tags = new List<Tag> { (Tag)"Tag 1", (Tag)"Tag 2" },
                //Thumbnail = "https://i.etsystatic.com/19002916/r/il/617e49/1885302518/il_fullxfull.1885302518_9ovo.jpg"
                //Thumbnail = (Thumbnail)@"D:\__MANAGED_FILES__\DnD\__Thumbnails__\20200126_191331.jpg"
            };
        }
        private ItemChangeset _getTaggyItem(string name)
        {
            return new ItemChangeset()
            {
                Name = name,
                Tags = new List<Tag> { (Tag)"Tag 0", (Tag)"Tag 1", (Tag)"Tag 2", (Tag)"Tag 3", (Tag)"Tag 4",
                                       (Tag)"Tag 5", (Tag)"Tag 6", (Tag)"Tag 7", (Tag)"Tag 8", (Tag)"Tag 9",
                                       (Tag)"Tag 10", (Tag)"Tag 11", (Tag)"Tag 12", (Tag)"Tag 13", (Tag)"Tag 14",
                                       (Tag)"Tag 15", (Tag)"Tag 16", (Tag)"Tag 17", (Tag)"Tag 18", (Tag)"Tag 19", },
                //Thumbnail = "https://i.etsystatic.com/19002916/r/il/617e49/1885302518/il_fullxfull.1885302518_9ovo.jpg"
                //Thumbnail = (Thumbnail)@"D:\__MANAGED_FILES__\DnD\__Thumbnails__\20200126_191331.jpg"
            };
        }
    }
}
