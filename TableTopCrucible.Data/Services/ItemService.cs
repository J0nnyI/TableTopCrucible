using DynamicData;

using System;
using System.Linq;
using System.Reactive.Linq;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using DynamicData.Alias;
using System.Reactive.Subjects;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Data.Models.Views;
using System.Collections.Generic;
using TableTopCrucible.Core.Helper;
using TableTopCrucible.Data.Models.ValueTypes;

namespace TableTopCrucible.Data.Services
{
    public interface IItemService : IDataService<Item, ItemId, ItemChangeset>
    {
        IObservableCache<ItemEx, ItemId> GetExtended();
        IObservable<ItemEx> GetExtended(ItemId item);
        IObservable<ItemEx?> GetExtended(IObservable<ItemId?> itemIdChanges);
        IObservable<IChangeSet<Tag>> GetTags(IObservable<IChangeSet<Item, ItemId>> sourceItems);
        IObservableList<Tag> GetTags();
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
            : base(settingsService, notificationCenterService)
        {
            this._fileService = fileInfoService;
            this.fileItemLinkService = fileItemLinkService;
            this._notificationCenterService = notificationCenterService;
            this._settingsService = settingsService;

            this._extended =
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

            this._tags = GetTags(Get().Connect()).AsObservableList();
        }

        private readonly IObservableCache<ItemEx, ItemId> _extended;
        public IObservableCache<ItemEx, ItemId> GetExtended()
            => _extended;

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

        private IObservableList<Tag> _tags;
        public IObservableList<Tag> GetTags()
            => _tags;
        public IObservable<IChangeSet<Tag>> GetTags(IObservable<IChangeSet<Item, ItemId>> sourceItems)
        {
            return sourceItems
                .TransformMany(item => item.Tags, tag => tag)
                .DistinctValues(tag => tag)
                .RemoveKey();
        }
    }
}
