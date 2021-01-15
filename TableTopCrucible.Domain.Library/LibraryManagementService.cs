using DynamicData;

using Microsoft.WindowsAPICodePack.Shell;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TableTopCrucible.Core.Enums;
using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Enums;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Data.Files.Scanner;
using TableTopCrucible.Data.Models.Sources;
using TableTopCrucible.Data.Models.ValueTypes;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

using FileInfo = TableTopCrucible.Data.Models.Sources.FileInfo;
using SysFileInfo = System.IO.FileInfo;
using Version = TableTopCrucible.Domain.Models.ValueTypes.Version;

namespace TableTopCrucible.Domain.Library

{
    public interface ILibraryManagementService
    {
        void AutoGenerateItems();
        void RemoveDirectorySetupRecursively(DirectorySetupId dirSetupId);
    }

    public class LibraryManagementService : ILibraryManagementService
    {

        private readonly IDirectoryDataService directoryDataService;
        private readonly IModelFileDataService modelFileDataService;
        private readonly IImageFileDataService imageFileDataService;
        private readonly IFileItemLinkService fileItemLinkService;
        private readonly IItemDataService itemService;
        private readonly INotificationCenterService notificationCenter;
        private readonly ISettingsService settingsService;
        private readonly IFileManagementService fileManagementService;

        public LibraryManagementService(
            IDirectoryDataService directoryDataService,
            IModelFileDataService modelFileDataService,
            IImageFileDataService imageFileDataService,
            IFileItemLinkService fileItemLinkService,
            IItemDataService itemService,
            INotificationCenterService notificationCenter,
            ISettingsService settingsService,
            IFileManagementService fileManagementService)
        {
            this.directoryDataService = directoryDataService;
            this.modelFileDataService = modelFileDataService;
            this.imageFileDataService = imageFileDataService;
            this.fileItemLinkService = fileItemLinkService;
            this.itemService = itemService;
            this.notificationCenter = notificationCenter;
            this.settingsService = settingsService;
            this.fileManagementService = fileManagementService;
        }

        public void AutoGenerateItems()
        {
            Observable.Start(() =>
            {
                var job = this.notificationCenter.CreateSingleTaskJob(out var process, "creating items", "preparing");
                try
                {
                    process.State = AsyncState.InProgress;
                    var object3dExtensions = new string[] { ".obj", ".stl" };
                    var threadcount = settingsService.ThreadCount;

                    var files = this.modelFileDataService
                        .GetExtendedByHash()
                        .KeyValues
                        .Where(x =>
                            object3dExtensions.Contains(
                                Path.GetExtension(x.Value.AbsolutePath)))
                        .ToDictionary(x => x.Key, x => x.Value);

                    var items = this.itemService
                        .GetExtended()
                        .Items;

                    var takenKeys = items
                        .Where(x => x.FileVersions.Any())
                        .SelectMany(x => x.FileVersions)
                        .Select(x => x.Link.FileKey);

                    var knownKeys = files
                        .Select(x => x.Key);

                    var freeKeys = knownKeys
                        .Except(takenKeys);
                    process.Details = $"preparing {freeKeys.Count()} items";

                    var patches = freeKeys
                        .Select(freeKey =>
                        {
                            var file = files[freeKey];

                            var item = new Item((ItemName)Path.GetFileNameWithoutExtension(file.AbsolutePath), new Tag[] { (Tag)"new" });

                            var link = new FileItemLink(item.Id, file.HashKey.Value, null, new Version(1, 0, 0));

                            return new { item, link };
                        }).ToArray();

                    process.Details = $"posting {items.Count()} items";

                    this.itemService.Post(patches.Select(x => x.item), RxApp.TaskpoolScheduler);

                    this.fileItemLinkService.Post(patches.Select(x => x.link), RxApp.TaskpoolScheduler);

                    process.State = AsyncState.Done;
                }
                catch (Exception ex)
                {
                    process.State = AsyncState.Failed;
                    MessageBox.Show(ex.ToString(), $"{nameof(LibraryManagementService)}.{nameof(AutoGenerateItems)}");
                }
                finally
                {

                }

            }, RxApp.TaskpoolScheduler);

        }


        public void RemoveDirectorySetupRecursively(DirectorySetupId dirSetupId)
        {
            Observable.Start(() =>
            {
                var job = notificationCenter.CreateSingleTaskJob(out var process, "removing directory setup");
                try
                {
                    process.AddProgress(7, "removing setup");
                    this.directoryDataService.Delete(dirSetupId);

                    process.OnNextStep("looking for files");
                    var files =
                        modelFileDataService
                        .Get()
                        .Items
                        .Where(file => file.DirectorySetupId == dirSetupId)
                        .ToList();

                    process.OnNextStep("removing files");
                    modelFileDataService
                        .Delete(
                            files
                            .Select(file => file.Id)
                        );

                    process.OnNextStep("looking for links");
                    var hashes = files.Select(x => x.HashKey);
                    files = null;

                    var removedLinks =
                        fileItemLinkService
                        .Get()
                        .Items
                        .WhereIn(hashes, link => link.FileKey)
                        .ToArray();

                    process.OnNextStep("removing links");
                    fileItemLinkService
                        .Delete(
                            removedLinks
                            .Select(link => link.Id)
                        );

                    process.OnNextStep("looking for items");
                    var remainingFileKeys =
                        fileItemLinkService
                        .Get()
                        .Items
                        .Select(x => x.FileKey);

                    var completelyRemovedItemIDs =
                        removedLinks
                        .WhereNotIn(remainingFileKeys, link => link.FileKey)
                        .Select(link => link.ItemId)
                        .ToArray();

                    removedLinks = null;

                    process.OnNextStep("removing items");
                    this.itemService.Delete(completelyRemovedItemIDs);
                    process.OnNextStep("done");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), $"{nameof(LibraryManagementService)}.{nameof(RemoveDirectorySetupRecursively)}");
                }
            }, RxApp.TaskpoolScheduler);
        }
    }
}
