using DynamicData;
using DynamicData.Kernel;

using System;
using System.IO;
using FileInfo = TableTopCrucible.Domain.Models.Sources.FileInfo;
using System.Linq;
using System.Reactive.Linq;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using TableTopCrucible.Domain.Models.Views;
using DynamicData.List;
using DynamicData.Alias;
using System.Windows.Documents;
using System.Reactive.Subjects;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.ValueTypes;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using ReactiveUI;
using TableTopCrucible.Core.Models.Enums;
using TableTopCrucible.Data.Models.Views;

namespace TableTopCrucible.Data.Services
{
    public interface IItemService : IDataService<Item, ItemId, ItemChangeset>
    {
        public IObservableCache<ExtendedItem, ItemId> GetExtended();
        public IObservable<ExtendedItem?> GetExtended(IObservable<ItemId?> itemIdChanges);

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
                .LeftJoinMany<Item, ItemId, VersionedFile, FileInfoHashKey, ExtendedItem>(
                    this.fileItemLinkService
                        .GetVersionedFilesByHash()
                        .Connect(),
                    (VersionedFile file) => file.Link.ItemId,
                    (item, files) => new ExtendedItem(item, files.Items))
                .ChangeKey(item => item.Item.Id)
                .AsObservableCache();
        }

        private readonly IObservableCache<ExtendedItem, ItemId> _getExtended;
        public IObservableCache<ExtendedItem, ItemId> GetExtended()
            => _getExtended;

        public IObservable<ExtendedItem?> GetExtended(IObservable<ItemId?> itemIdChanges)
        {
            return itemIdChanges.Select(id => id != null ?
                this.GetExtended()
                .WatchValue(id.Value)
                .Select(x => x as ExtendedItem?) :
                new BehaviorSubject<ExtendedItem?>(null))
                .Switch()
                .TakeUntil(destroy);
        }

    }
}
