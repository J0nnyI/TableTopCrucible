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
        private readonly IUiDispatcherService _dispatcherService;

        public ItemService(IFileDataService fileInfoService, INotificationCenterService notificationCenterService, ISettingsService settingsService, IUiDispatcherService dispatcherService)
        {
            this._fileService = fileInfoService;
            this._notificationCenterService = notificationCenterService;
            this._settingsService = settingsService;
            this._dispatcherService = dispatcherService;

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
            #region link items with files
                this.Get()
                .Connect()
                .Filter(x => x.File.HasValue)
                .ChangeKey(x => x.File)
                .LeftJoinMany(
                    this._fileService.GetExtendedByHash().Connect(),
                    file => file.HashKey,
                    (item, files) => new ExtendedItem(item, files.Items))
                .ChangeKey(x => x.Item.Id)
            #endregion
                .FullJoin(// basically a concat
            #region get items without files and format them
                    this.Get()
                    .Connect()
                    .Filter(x => !x.File.HasValue)
                    .Transform(item => new ExtendedItem(item, new ExtendedFileInfo[0])),
            #endregion
                    x => x.Item.Id,
                    (x, y) => x.HasValue ? x.Value : y.Value)
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
            IEnumerable<ItemChangeset> changesets = null;

            //var job = new AsyncJob("Item Generator", feedback =>
            //{

            new Thread(() =>
            {
                try
                {
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

                    changesets = freeKeys
                        .Select(freeKey =>
                        {
                            var file = files[freeKey];

                            return new ItemChangeset()
                            {
                                Name = Path.GetFileNameWithoutExtension(file.AbsolutePath),
                                Tags = new Tag[] { (Tag)"autogenerated" },
                            };
                        }).ToArray();
                    AsyncProcessState process = null;
                    this._dispatcherService.UiDispatcher.Invoke(() =>
                    {

                        var job = this._notificationCenterService.CreateSingleTaskJob(out process, "creating items", "", this._dispatcherService.UiDispatcher);
                        process.AddProgress(changesets.Count() / _settingsService.MaxPatchSize + 1);
                    });

                    for (int i = 0; i < changesets.Count(); i += _settingsService.MaxPatchSize)
                    {
                        var subpatch = changesets.Skip(i).Take(_settingsService.MaxPatchSize);
                        this._dispatcherService.UiDispatcher.Invoke(() =>
                        {
                            this.Patch(subpatch);
                        },DispatcherPriority.Background);
                        Thread.Sleep(1000);
                        process.OnNextStep();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            }).Start();

            //});


            //var prepThread = job.Starters.FirstOrDefault();
            //prepThread.AddSuccessor("saving",changesets, subset => this.Patch(subset), _settingsService.ThreadCount, _settingsService.MaxPatchSize);
            //job.Start();

        }
    }
}
