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

namespace TableTopCrucible.Data.Services
{
    public interface IItemService : IDataService<Item, ItemId, ItemChangeset>
    {
        public void AutoGenerateItems();
        public IObservableCache<ExtendedItem, ItemId> GetExtended();
        public IObservable<ExtendedItem?> GetExtended(IObservable<ItemId?> itemIdChanges);

    }
    public class ItemService : DataServiceBase<Item, ItemId, ItemChangeset>, IItemService
    {

        private IFileDataService _fileService;
        private readonly INotificationCenterService _notificationCenterService;
        private readonly ISettingsService _settingsService;

        public ItemService(
            IFileDataService fileInfoService,
            INotificationCenterService notificationCenterService,
            ISettingsService settingsService)
            :base(settingsService, notificationCenterService)
        {
            this._fileService = fileInfoService;
            this._notificationCenterService = notificationCenterService;
            this._settingsService = settingsService;

            // select ExtendedItem 
            // from extendedFiles 
            // left join items 
            // on items.file = extendedFiles.HaskKey
            this._getLatestItems = this._fileService
                .GetExtendedByHash()
                .Connect()
                .LeftJoin(
                    this.Get()
                        .Connect()
                        .Filter(item => item.File.HasValue),
                    (item) => item.File.Value,
                    (file, item) => { return new { file, item }; })
                .Filter(x => x.item.HasValue)
                .Transform(x => new ExtendedItem(x.item.Value, x.file))
                .ChangeKey(x => x.Item.Id)
                .TakeUntil(destroy)
                .AsObservableCache();

            this._getExtendedbyHash =
                this.Get()
                .Connect()
                .Filter(item => item.File.HasValue)
                .ChangeKey(item => item.File.Value)
                .LeftJoin(
                    this._fileService.GetExtendedByHash().Connect(),
                    (file) => file.HashKey.Value,
                    (item, file) => new ExtendedItem(item, file.Value))
                .TakeUntil(destroy)
                .AsObservableCache();

            this._getExtended =
                this.Get()
                .Connect()
                .Filter(x => x.File.HasValue)
                .ChangeKey(x => x.File)
                .LeftJoinMany(
                    this._fileService.GetExtendedByHash().Connect(),
                    file => file.HashKey,
                    (item, files) => new ExtendedItem(item, files.Items))
                .ChangeKey(x => x.Item.Id)
                .Merge(
                    Get()
                    .Connect()
                    .Filter(x => !x.File.HasValue)
                    .Transform(item => new ExtendedItem(item, new ExtendedFileInfo[0])))
                .AsObservableCache();

        }

        private readonly IObservableCache<ExtendedItem, ItemId> _getLatestItems;
        public IObservableCache<ExtendedItem, ItemId> GetLatestItems()
            => _getLatestItems;

        private readonly IObservableCache<ExtendedItem, FileInfoHashKey> _getExtendedbyHash;
        public IObservableCache<ExtendedItem, FileInfoHashKey> GetExtendedByHash()
            => _getExtendedbyHash;

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

        private static class _itemGenerator
        {
            public static void AutogenerateItems(ItemService itemService)
            {

            }
        }

        public void AutoGenerateItems()
        {

            //var job = new AsyncJob("Item Generator", feedback =>
            //{

            Observable.Start(() =>
            {
                var job = this._notificationCenterService.CreateSingleTaskJob(out var process, "creating items", "preparing");
                try
                {
                    process.State = AsyncState.InProgress;
                    var object3dExtensions = new string[] { ".obj", ".stl", ".3mf" };
                    var threadcount = _settingsService.ThreadCount;

                    var files = this._fileService
                        .GetExtendedByHash()
                        .KeyValues
                        .Where(x =>
                            object3dExtensions.Contains(
                                Path.GetExtension(x.Value.AbsolutePath)))
                        .ToDictionary(x => x.Key, x => x.Value);

                    var items = this
                        .Get()
                        .Items;

                    var takenKeys = items
                        .Where(x => x.File.HasValue)
                        .Select(x => x.File.Value);

                    var knownKeys = files
                        .Select(x => x.Key);

                    var s = Enumerable.Range(5, 10).Except(Enumerable.Range(3, 7));

                    var freeKeys = knownKeys
                        .Except(takenKeys);
                    process.Details = $"preparing {freeKeys.Count()} items";

                    items = freeKeys
                        .Select(freeKey =>
                        {
                            var file = files[freeKey];

                            return new Item((ItemName)Path.GetFileNameWithoutExtension(file.AbsolutePath), new Tag[] { (Tag)"autogenerated" });
                        }).ToArray();

                    //process.AddProgress(items.Count() / _settingsService.MaxPatchSize + 1);
                    process.Details = $"posting {items.Count()} items";

                    this.Post(items);

                    //for (int i = 0; i < items.Count(); i += _settingsService.MaxPatchSize)
                    //{
                    //    var subpatch = items.Skip(i).Take(_settingsService.MaxPatchSize);
                    //    this.Post(subpatch);
                    //    process.OnNextStep();
                    //}
                    process.State = AsyncState.Done;
                }
                catch (Exception ex)
                {
                    process.State = AsyncState.Failed;
                    MessageBox.Show(ex.ToString());
                }
                finally
                {

                }

            }, RxApp.TaskpoolScheduler);

            //});


            //var prepThread = job.Starters.FirstOrDefault();
            //prepThread.AddSuccessor("saving",changesets, subset => this.Patch(subset), _settingsService.ThreadCount, _settingsService.MaxPatchSize);
            //job.Start();

        }
    }
}
