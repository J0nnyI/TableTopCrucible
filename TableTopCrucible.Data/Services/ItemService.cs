using DynamicData;

using System;
using System.Linq;
using System.Reactive.Linq;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using DynamicData.Alias;
using System.Reactive.Subjects;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Data.Models.Views;
using System.Collections.Generic;
using TableTopCrucible.Core.Helper;
using TableTopCrucible.Data.Models.ValueTypes;
using TableTopCrucible.Data.Models.Sources;

namespace TableTopCrucible.Data.Services
{
    public interface IItemDataService : IDataService<Item, ItemId, ItemChangeset>
    {
        IObservableCache<ItemEx, ItemId> GetExtended();
        IObservable<ItemEx> GetExtended(ItemId item);
        IObservable<ItemEx?> GetExtended(IObservable<ItemId?> itemIdChanges);
        IObservable<IChangeSet<Tag>> GetTags(IObservable<IChangeSet<Item, ItemId>> sourceItems);
        IObservable<IChangeSet<Tag>> GetTags(IObservable<Func<ItemEx, bool>> itemFilter);
        IObservableList<Tag> GetTags();
        IObservable<IChangeSet<ThumbnailItem, ItemId>> GetThumbnailItems();
    }
    public class ItemService : DataServiceBase<Item, ItemId, ItemChangeset>, IItemDataService
    {

        private IFileDataService _fileService;
        private readonly IFileItemLinkService fileItemLinkService;
        private readonly INotificationCenterService _notificationCenterService;
        private readonly ISettingsService _settingsService;
        private readonly IObservable<IChangeSet<ThumbnailItem, ItemId>> _thumbnailItems;
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

            this._extended = this
                .Get()
                .Connect()
                .LeftJoinMany(
                    this.fileItemLinkService
                        .GetVersionedFiles()
                        .Connect(),
                    (VersionedFile file) => file.Link.ItemId,
                    (item, files) => new ItemEx(item, files.Items))
                .ChangeKey(item => item.SourceItem.Id)
                .AsObservableCache();


            var thumbnails = fileItemLinkService
                .Get()
                .Connect()
                .Filter(link=>link.ThumbnailKey.HasValue)
                .GroupWithImmutableState(link=>link.ItemId)
                .Transform(links=>links.Items.MaxBy(link=>link.Version))
                .Filter(link=>link.HasValue)
                .Transform(link=>link.Value)
                .ChangeKey(link=>link.ThumbnailKey.Value)
                .LeftJoin(
                    this._fileService
                        .GetExtended()
                        .Connect()
                        .Filter(file=>file.HashKey.HasValue),
                    file => file.HashKey.Value,
                    (link, file) => { return new { link, file=file.Value }; }
                );


            _thumbnailItems = this
                .Get()
                .Connect()
                .LeftJoin(
                    thumbnails,
                    x=> x.link.ItemId,
                    (item, x) => new ThumbnailItem(item,x.Value.link, x.Value.file)
                );


            this._tags = GetTags(Get().Connect()).AsObservableList();
        }

        private readonly IObservableCache<ItemEx, ItemId> _extended;
        public IObservableCache<ItemEx, ItemId> GetExtended()
            => _extended;

        public IObservable<IChangeSet<ThumbnailItem, ItemId>> GetThumbnailItems() => _thumbnailItems;

        public IObservable<ItemEx?> GetExtended(IObservable<ItemId?> itemIdChanges)
        {
            return itemIdChanges.Select(id =>
                id != null
                ?
                this.GetExtended()
                    .WatchValue(id.Value)
                    .Select(x => x as ItemEx?)
                :
                Observable.Return<ItemEx?>(null)
                )
                .Switch()
                .TakeUntil(destroy);
        }
        public IObservable<ItemEx> GetExtended(ItemId item)
            => this.GetExtended().WatchValue(item);

        private IObservableList<Tag> _tags;
        public IObservableList<Tag> GetTags()
            => _tags;
        public IObservable<IChangeSet<Tag>> GetTags(IObservable<IChangeSet<ItemEx, ItemId>> sourceItems)
            => GetTags(sourceItems.Transform(itemEx => itemEx.SourceItem));
        public IObservable<IChangeSet<Tag>> GetTags(IObservable<IChangeSet<Item, ItemId>> sourceItems)
        {
            return sourceItems
                .TransformMany(item => item.Tags, tag => tag)
                .DistinctValues(tag => tag)
                .RemoveKey();
        }


        public IObservable<IChangeSet<Tag>> GetTags(IObservable<Func<ItemEx, bool>> itemFilter)
            => GetTags(this.GetExtended().Connect().Filter(itemFilter));
    }
}
