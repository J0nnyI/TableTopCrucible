using DynamicData;

using System;
using System.Linq;
using System.Reactive.Linq;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using DynamicData.Alias;
using System.Reactive.Subjects;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Data.Models.Views;
using System.Collections.Generic;

namespace TableTopCrucible.Data.Services
{
    public interface IItemService : IDataService<Item, ItemId, ItemChangeset>
    {
        public IObservableCache<ItemEx, ItemId> GetExtended();
        public IObservable<ItemEx> GetExtended(ItemId item);
        public IObservable<ItemEx?> GetExtended(IObservable<ItemId?> itemIdChanges);
    }
    public class ItemService : DataServiceBase<Item, ItemId, ItemChangeset>, IItemService
    {

        private IFileDataService _fileService;
        private readonly IFileItemLinkService fileItemLinkService;
        private readonly INotificationCenterService _notificationCenterService;
        private readonly ISettingsService _settingsService;

        public ItemService(
            IFileDataService fileInfoService,
            IFileItemLinkService fileItemLinkService,
            INotificationCenterService notificationCenterService,
            ISettingsService settingsService)
            :base(settingsService, notificationCenterService)
        {
            this._fileService = fileInfoService;
            this.fileItemLinkService = fileItemLinkService;
            this._notificationCenterService = notificationCenterService;
            this._settingsService = settingsService;

            this._getExtended =
                this.Get()
                .Connect()
                .LeftJoinMany<Item, ItemId, VersionedFile, FileItemLinkId, ItemEx>(
                    this.fileItemLinkService
                        .GetVersionedFiles()
                        .Connect(),
                    (VersionedFile file) => file.Link.ItemId,
                    (item, files) => new ItemEx(item, files.Items))
                .ChangeKey(item => item.SourceItem.Id)
                .AsObservableCache();
        }

        private readonly IObservableCache<ItemEx, ItemId> _getExtended;
        public IObservableCache<ItemEx, ItemId> GetExtended()
            => _getExtended;

        public IObservable<ItemEx?> GetExtended(IObservable<ItemId?> itemIdChanges)
        {
            return itemIdChanges.Select(id => id != null ?
                this.GetExtended()
                .WatchValue(id.Value)
                .Select(x => x as ItemEx?) :
                new BehaviorSubject<ItemEx?>(null))
                .Switch()
                .TakeUntil(destroy);
        }
        public IObservable<ItemEx> GetExtended(ItemId item)
            => this.GetExtended().WatchValue(item);

    }
}
