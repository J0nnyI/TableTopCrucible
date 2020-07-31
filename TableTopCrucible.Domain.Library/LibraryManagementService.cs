using ReactiveUI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows;

using TableTopCrucible.Core.Models.Enums;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using TableTopCrucible.WPF.Helper;

using SysFileInfo = System.IO.FileInfo;
using Version = TableTopCrucible.Domain.Models.ValueTypes.Version;
namespace TableTopCrucible.Domain.Library

{
    public interface ILibraryManagementService
    {
        void AutoGenerateItems();
        void SynchronizeFiles();
    }

    public class LibraryManagementService : ILibraryManagementService
    {
        private struct LocalFile
        {
            public LocalFile(DirectorySetup directorySetup, string localPath)
            {
                DirectorySetup = directorySetup;
                FileInfo = new SysFileInfo(localPath);
            }

            public DirectorySetup DirectorySetup { get; }
            public SysFileInfo FileInfo { get; }
        }

        private readonly IDirectoryDataService directoryDataService;
        private readonly IFileDataService fileDataService;
        private readonly IFileItemLinkService fileItemLinkService;
        private readonly IItemService itemService;
        private readonly INotificationCenterService notificationCenter;
        private readonly ISettingsService settingsService;

        public LibraryManagementService(
            IDirectoryDataService directoryDataService,
            IFileDataService fileDataService,
            IFileItemLinkService fileItemLinkService,
            IItemService itemService,
            INotificationCenterService notificationCenter,
            ISettingsService settingsService)
        {
            this.directoryDataService = directoryDataService;
            this.fileDataService = fileDataService;
            this.fileItemLinkService = fileItemLinkService;
            this.itemService = itemService;
            this.notificationCenter = notificationCenter;
            this.settingsService = settingsService;
        }

        #region file sync
        private IEnumerable<ExtendedFileInfo> getDefinedFiles()
        {
            return fileDataService
                .GetExtended()
                .KeyValues
                .Select(x => x.Value);
        }

        private IEnumerable<LocalFile> getLocalFiles(DirectorySetup directorySetup)
        {
            return Directory.GetFiles(directorySetup.Path.LocalPath, "*", SearchOption.AllDirectories)
                .Select(file => new LocalFile(directorySetup, file));
        }

        private IEnumerable<LocalFile> getLocalFiles(IEnumerable<DirectorySetup> dirSetups)
            => dirSetups.SelectMany(setup => getLocalFiles(setup));

        private IEnumerable<string> getDistinctPaths(IEnumerable<LocalFile> localFiles, IEnumerable<ExtendedFileInfo> definedFiles)
        {
            var localPaths = localFiles.Select(file => file.FileInfo.FullName);
            var definedPaths = definedFiles.Select(file => file.AbsolutePath);
            return localPaths.Concat(definedPaths).Distinct();
        }

        private IEnumerable<DirectorySetup> getDirSetups()
            => this.directoryDataService.Get().KeyValues.Select(x => x.Value);
        private FileInfoChangeset synchronizeFile(LocalFile? localFile, ExtendedFileInfo? definedFile)
        {
            if (localFile.HasValue && definedFile.HasValue)
            {
                if (localFile?.DirectorySetup != definedFile?.DirectorySetup)
                    throw new Exception($"dir setups do not match up for file '{localFile?.FileInfo?.FullName}' / '{definedFile?.AbsolutePath}'");
                if (localFile?.FileInfo?.FullName != definedFile?.AbsolutePath)
                    throw new Exception($"path do not match up for file '{localFile?.FileInfo?.FullName}' / '{definedFile?.AbsolutePath}'");
            }


            var result = new FileInfoChangeset(definedFile?.FileInfo);
            var dirSetup = (definedFile?.DirectorySetup ?? localFile?.DirectorySetup).Value;

            // no filechange
            if (localFile.HasValue && definedFile.HasValue && localFile?.FileInfo?.LastWriteTime == definedFile?.LastWriteTime)
                return null;

            // file was deleted
            result.IsAccessible = localFile.HasValue;

            var path = definedFile?.AbsolutePath ?? localFile?.FileInfo?.FullName;

            result.SetSysFileInfo(dirSetup, localFile?.FileInfo);
            result.Path = dirSetup.Path.MakeUnescapedRelativeUri(path);
            result.DirectorySetupId = localFile?.DirectorySetup.Id ?? definedFile.Value.DirectorySetup.Id;
            result.Id = definedFile?.FileInfo.Id ?? FileInfoId.New();

            return result;
        }
        public void SynchronizeFiles()
        {
            notificationCenter.CreateSingleTaskJob(out var processState, $"working...", "locating files");
            try
            {
                processState.State = AsyncState.InProgress;
                var dirSetups = getDirSetups();
                var definedFiles = getDefinedFiles().ToArray();
                var localFiles = getLocalFiles(dirSetups).ToArray();
                var paths = getDistinctPaths(localFiles, definedFiles);

                var mergedFiles =
                    (from file in paths
                     join localFile in localFiles
                         on file equals localFile.FileInfo.FullName into mLocalFile
                     join definedFile in definedFiles
                         on file equals definedFile.AbsolutePath into mDefinedFile
                     select synchronizeFile(mLocalFile.Any() ? mLocalFile.First() as LocalFile? : null, mDefinedFile.Any() ? mDefinedFile.First() as ExtendedFileInfo? : null))
                    .Where(x => x != null);

                var changesets = mergedFiles.Where(x => x.IsAccessible).ToArray();
                var deleteSets = mergedFiles.Where(x => !x.IsAccessible).Select(x => x.Id).ToArray();
                processState.Details = $"patching: located {changesets.Length} files, detected {deleteSets.Length} deleted files";
                fileDataService.Patch(changesets);
                fileDataService.Delete(deleteSets);
                processState.State = AsyncState.Done;
                processState.Details = $"done. located {changesets.Length} files, detected {deleteSets.Length} deleted files";
            }
            catch (Exception ex)
            {
                processState.State = AsyncState.Failed;
                processState.Details = ex.ToString();
            }
        }
        #endregion

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

                    var files = this.fileDataService
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
                        .Where(x => x.Files.Any())
                        .SelectMany(x => x.Files)
                        .Select(x => x.Link.FileKey);

                    var knownKeys = files
                        .Select(x => x.Key);

                    var s = Enumerable.Range(5, 10).Except(Enumerable.Range(3, 7));

                    var freeKeys = knownKeys
                        .Except(takenKeys);
                    process.Details = $"preparing {freeKeys.Count()} items";

                    var patches = freeKeys
                        .Select(freeKey =>
                        {
                            var file = files[freeKey];

                            var item = new Item((ItemName)Path.GetFileNameWithoutExtension(file.AbsolutePath), new Tag[] { (Tag)"new" });

                            var link = new FileItemLink(item.Id, file.HashKey.Value, new Version(1, 0, 0));

                            return new { item, link };
                        }).ToArray();

                    process.Details = $"posting {items.Count()} items";

                    Observable.Start(() =>
                        this.itemService.Post(patches.Select(x => x.item)),
                        RxApp.TaskpoolScheduler);

                    Observable.Start(() =>
                        this.fileItemLinkService.Post(patches.Select(x => x.link)),
                    RxApp.TaskpoolScheduler);

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

        }
    }
}
