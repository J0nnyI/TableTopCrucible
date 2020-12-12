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
        void FullSync();
        void AutoGenerateItems();
        FileInfo? UpdateFile(DirectorySetup dirSetup, Uri relativePath);
        void RemoveDirectorySetupRecursively(DirectorySetupId dirSetupId);
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


        public static Version GetNextVersion(Version previousVersion, IEnumerable<Version> takenVersions)
        {
            var nextMajor = previousVersion.Major + 1;
            var nextMinor = previousVersion.Minor + 1;
            var nextPatch = previousVersion.Patch + 10000;

            if (!takenVersions.Any(x =>
                    x.Major == nextMajor))
                return new Version(nextMajor, 0, 0);

            if (!takenVersions.Any(x =>
                    x.Major == previousVersion.Major &&
                    x.Minor == nextMinor))
                return new Version(previousVersion.Major, nextMinor, 0);



            var patchRange = takenVersions.Where(x =>
                    x.Major == previousVersion.Major &&
                    x.Minor == previousVersion.Minor &&
                    x.Patch.Between(previousVersion.Patch + 1, nextPatch));

            if (!patchRange.Any())
                return new Version(previousVersion.Major, previousVersion.Minor, nextPatch);

            var nextTakenVersion = patchRange.Min();

            var fallbackPatch = (nextTakenVersion.Patch - previousVersion.Patch) / 2 + previousVersion.Patch;

            return new Version(previousVersion.Major, previousVersion.Minor, fallbackPatch);
        }
        private struct FilePatch
        {
            public FilePatch(bool isUpdate, DirectorySetup directorySetup, SysFileInfo fileInfo, FileHash hash)
            {
                IsUpdate = isUpdate;
                DirectorySetup = directorySetup;
                FileInfo = fileInfo;
                Hash = hash;
            }

            public bool IsUpdate { get; }
            public DirectorySetup DirectorySetup { get; }
            public SysFileInfo FileInfo { get; }
            public FileHash Hash { get; }
        }
        public void FullSync()
        {
            //FullSync(getDirSetups(), this.modelFileDataService);

            _getFiles(
                modelFileDataService,
                new string[] { "stl", "obj", "off", "objz", "lwo", "3ds" },
                out var deletedModelFiles,
                out var updatedModelFiles,
                out var newModelFiles);
            var modelUpdateHashes = _hashFiles(updatedModelFiles.Select(file => file.AbsolutePath));
            var newModelHashes = _hashFiles(newModelFiles.Select(file => file.FileInfo.FullName));
            modelFileDataService.Delete(deletedModelFiles.Select(file => file.Id));

            _getFiles(
                imageFileDataService,
                new string[] { "png", "jpg", "jpeg", "bmp", "gif", "hdp", "jp2", "pbm", "psd", "tga", "tiff", "img" },
                out var deletedImageFiles,
                out var updatedImageFiles,
                out var newImageFiles);
            var imageUpdateHashes = _hashFiles(updatedImageFiles.Select(file => file.AbsolutePath));
            var newImageHashes = _hashFiles(newImageFiles.Select(file => file.FileInfo.FullName));
            imageFileDataService.Delete(deletedModelFiles.Select(file => file.Id));
        }
        private void createFileInfo(IDictionary<string, FileHash>)
        {

        }
        private void _handleItemUpdate(IEnumerable<FileInfoEx> files)
        {
            var links = fileItemLinkService.Get().Items;
            //files.Select(file =>
            //{
            //    //links.Where(link => link.FileKey ==  )
            //});

            foreach (FileInfoEx file in files)
            {

            }
        }
        private void _getFiles(
            IFileDataService fileService,
            IEnumerable<string> fileExtensions,
            out IEnumerable<FileInfoEx> deletedFiles,
            out IEnumerable<FileInfoEx> updatedFiles,
            out IEnumerable<LocalFile> newFiles)
        {
            var dirSetups = this.directoryDataService.Get().Items;
            var localFiles = getLocalFiles(dirSetups, fileExtensions);
            var definedFiles = getDefinedFiles(dirSetups, fileService);
            var paths = getDistinctPaths(localFiles, definedFiles);

            var allFiles = (from file in paths
                            join localFile in localFiles
                                on file.ToLower() equals localFile.FileInfo.FullName.ToLower() into mLocalFile
                            join definedFile in definedFiles
                                on file.ToLower() equals definedFile.AbsolutePath.ToLower() into mDefinedFile
                            select new
                            {
                                localFile = mLocalFile.Any() ? (mLocalFile.First() as LocalFile?) : null,
                                definedFile = mDefinedFile.Any() ? mDefinedFile.First() as FileInfoEx? : null,
                                isNew = mLocalFile.Any() && !mDefinedFile.Any(),
                                isDeleted = !mLocalFile.Any() & mDefinedFile.Any(),
                                isUpdated = mLocalFile.Any() && mDefinedFile.Any() && mLocalFile.First().FileInfo.LastWriteTime != mDefinedFile.First().LastWriteTime || !mDefinedFile.FirstOrDefault().HashKey.HasValue,
                                unchanged = mLocalFile.Any() && mDefinedFile.Any() && mLocalFile.First().FileInfo.LastWriteTime == mDefinedFile.First().LastWriteTime
                            }).ToArray();

            deletedFiles = allFiles.Where(file => file.isDeleted).Select(file => file.definedFile.Value);
            updatedFiles = allFiles.Where(file => file.isUpdated).Select(file => file.definedFile.Value);
            newFiles = allFiles.Where(file => file.isNew).Select(file => file.localFile.Value);
        }

        private IDictionary<string, FileHash> _hashFiles(
            IEnumerable<string> files
            )
        {
            var res = new Dictionary<string, FileHash>();
            files.ToList().ForEach(file =>
            {
                var hash = fileManagementService.HashFile(file);
                res.Add(file, hash);
            });
            return res;
        }
        public void deleteFiles(IFileDataService fileService, IEnumerable<FileInfo> files)
        {

        }
        public void FullSync(IEnumerable<DirectorySetup> dirSetups, IFileDataService fileService, params string[] fileExtensions)
        {
            Observable.Start(() =>
            {
                try
                {
                    var job = new AsyncJobState("Full File Sync");
                    var mainProc = new AsyncProcessState("collecting data");
                    mainProc.AddProgress(3);
                    job.AddProcess(mainProc);
                    notificationCenter.Register(job);

                    mainProc.State = AsyncState.InProgress;


                    var localFiles = getLocalFiles(dirSetups);
                    var definedFiles = getDefinedFiles(dirSetups, fileService);


                    var paths = getDistinctPaths(localFiles, definedFiles);


                    var allFiles = (from file in paths
                                    join localFile in localFiles
                                        on file.ToLower() equals localFile.FileInfo.FullName.ToLower() into mLocalFile
                                    join definedFile in definedFiles
                                        on file.ToLower() equals definedFile.AbsolutePath.ToLower() into mDefinedFile
                                    select new
                                    {
                                        localFile = mLocalFile.Any() ? (mLocalFile.First() as LocalFile?) : null,
                                        definedFile = mDefinedFile.Any() ? mDefinedFile.First() as FileInfoEx? : null,
                                        isNew = mLocalFile.Any() && !mDefinedFile.Any(),
                                        isDeleted = !mLocalFile.Any() & mDefinedFile.Any(),
                                        isUpdated = mLocalFile.Any() && mDefinedFile.Any() && mLocalFile.First().FileInfo.LastWriteTime != mDefinedFile.First().LastWriteTime || !mDefinedFile.FirstOrDefault().HashKey.HasValue,
                                        unchanged = mLocalFile.Any() && mDefinedFile.Any() && mLocalFile.First().FileInfo.LastWriteTime == mDefinedFile.First().LastWriteTime
                                    }).ToArray();

                    // stage 1: hash new Files
                    mainProc.OnNextStep("handling file updates");

                    var filesToHash = allFiles
                        .Where(x => x.isNew || x.isUpdated);

                    if (filesToHash.Any())
                    {

                        var updateProc = new AsyncProcessState("hashing files file updates");
                        updateProc.AddProgress(6);
                        job.AddProcess(updateProc);

                        var chunks = filesToHash
                            .SplitEvenly(settingsService.ThreadCount);

                        var hashProcs = chunks
                            .Select(chunk =>
                            {
                                Subject<IEnumerable<FilePatch>> result = new Subject<IEnumerable<FilePatch>>();
                                RxApp.TaskpoolScheduler.Schedule(() =>
                                {
                                    var proc = new AsyncProcessState($"hashing #{chunk.Key}");
                                    proc.AddProgress(chunk.Count());
                                    lock (job)
                                        job.AddProcess(proc);
                                    proc.State = AsyncState.InProgress;

                                    var hasher = SHA512.Create();

                                    var res = chunk.Select(file =>
                                    {
                                        var res = new FilePatch(
                                            file.definedFile.HasValue,
                                            file.localFile.Value.DirectorySetup,
                                            file.localFile.Value.FileInfo,
                                            fileManagementService.HashFile(hasher, file.localFile.Value.FileInfo.FullName)
                                        );
                                        proc.OnNextStep();
                                        return res;
                                    }).ToArray();
                                    proc.State = AsyncState.Done;
                                    result.OnNext(res);
                                });
                                return result;
                            })
                            .ToArray();
                        hashProcs
                        .CombineLatest()
                        .Take(1)
                        .Subscribe(procRes =>
                        {

                            //var procRes = await hashProcs.CombineLatest();
                            var hashedFiles = procRes
                            .SelectMany(x => x)
                            .GroupBy(x => x.IsUpdate)
                            .ToArray();

                            updateProc.OnNextStep("preparing data");

                            var filesToUpdate = hashedFiles
                                .FirstOrDefault(x => x.Key == true)
                                ?.Select(x => new { x.DirectorySetup, x.FileInfo, x.Hash, x.IsUpdate, HashKey = new FileInfoHashKey(x.Hash, x.FileInfo.Length) })
                                ?.Join(
                                    allFiles.Where(x => x.isUpdated),
                                    x => x.FileInfo.FullName,
                                    x => x.localFile?.FileInfo?.FullName,
                                    (localFile, definedFile) => new
                                    {
                                        newHash = localFile.HashKey,
                                        newFileInfo = localFile.FileInfo,
                                        definedFile = definedFile.definedFile.Value
                                    })
                                ?.GroupBy(x => x.newHash);

                            var fileUpdateChangesets = new List<FileInfoChangeset>();
                            var fileItemLinkChangesets = new List<FileItemLinkChangeset>();
                            if (filesToUpdate != null)
                            {
                                updateProc.OnNextStep("creating linkupdates");
                                foreach (var files in filesToUpdate)
                                {
                                    // prepare links
                                    var relatedLinks = this.fileItemLinkService
                                        .Get()
                                        .KeyValues
                                        .Where(x => files.Any(y => y.definedFile.HashKey == x.Value.FileKey || y.definedFile.HashKey == x.Value.FileKey));
                                    foreach (var link in relatedLinks)
                                    {
                                        var versions = this.fileItemLinkService.Get().Items.Where(x => x.ItemId == link.Value.ItemId).Select(x => x.Version);

                                        var version = GetNextVersion(link.Value.Version, versions);


                                        var newLink = new FileItemLinkChangeset()
                                        {
                                            Version = version,
                                            ThumbnailKey = link.Value.ThumbnailKey,
                                            ItemId = link.Value.ItemId,
                                            FileKey = link.Value.FileKey,
                                        };

                                        if (link.Value.FileKey == files.FirstOrDefault()?.definedFile.HashKey)
                                        {
                                            newLink.FileKey = files.FirstOrDefault().newHash;
                                        }
                                        if (link.Value.ThumbnailKey == files.FirstOrDefault()?.definedFile.HashKey)
                                        {
                                            newLink.ThumbnailKey = files.FirstOrDefault().newHash;
                                        }

                                        fileItemLinkChangesets.Add(newLink);
                                    }

                                    // prepare file

                                    updateProc.OnNextStep("creating fileupdates");
                                    fileUpdateChangesets.AddRange(
                                        files.Select(file =>
                                        {
                                            var changeset = new FileInfoChangeset(file.definedFile.FileInfo);
                                            changeset.SetSysFileInfo(file.definedFile.DirectorySetup, file.newFileInfo);
                                            changeset.FileHash = file.newHash.FileHash;
                                            changeset.DirectorySetupId = file.definedFile.DirectorySetup.Id;
                                            changeset.Id = FileInfoId.New();
                                            return changeset;
                                        }));

                                }

                                updateProc.OnNextStep("patching...");
                                fileService.Patch(fileUpdateChangesets);
                                this.fileItemLinkService.Patch(fileItemLinkChangesets);
                            }
                            else
                            {
                                updateProc.Skip(3, "no merge required");
                            }

                            updateProc.OnNextStep("handling new files");
                            var newFiles = hashedFiles
                                .FirstOrDefault(x => x.Key == false)
                                ?.Select(file =>
                                {
                                    var changeset = new FileInfoChangeset();
                                    changeset.SetSysFileInfo(file.DirectorySetup, file.FileInfo);
                                    changeset.FileHash = file.Hash;
                                    changeset.DirectorySetupId = file.DirectorySetup.Id;
                                    changeset.Id = FileInfoId.New();
                                    return changeset;
                                })
                                .ToArray();

                            if (newFiles != null)
                                fileService.Patch(newFiles);

                            updateProc.OnNextStep("done");
                            updateProc.State = AsyncState.Done;
                        },
                        (Exception ex) =>
                        {
                            updateProc.State = AsyncState.Failed;
                            updateProc.Title = "failed";
                            updateProc.Details = ex.ToString();
                        });
                    }


                    // stage 3: remove deleted Files
                    mainProc.OnNextStep("deleting old files");

                    fileService.Delete(allFiles.Where(x => x.isDeleted).Select(x => x.definedFile.Value.Id));
                    mainProc.OnNextStep("done");
                    mainProc.State = AsyncState.Done;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), $"{nameof(LibraryManagementService)}.{nameof(FullSync)}");
                }
            }, RxApp.TaskpoolScheduler);
        }



        #region file sync utils

        private static IEnumerable<FileInfoEx> getDefinedFiles(IEnumerable<DirectorySetup> dirSetups, IFileDataService dataService)
        {
            var dirIDs = dirSetups.Select(x => x.Id);
            return dataService.GetExtended()
                .KeyValues
                .Select(x => x.Value)
                .Where(file => dirIDs
                .Contains(file.DirectorySetup.Id));
        }


        private static IEnumerable<LocalFile> getLocalFiles(DirectorySetup directorySetup, IEnumerable<string> fileExtensions)
        {
            return Directory
                .EnumerateFiles(directorySetup.Path.LocalPath, "*", SearchOption.AllDirectories)
                .Where(file => fileExtensions.Contains(Path.GetExtension(file)))
                .Select(file => new LocalFile(directorySetup, file));
        }

        private static IEnumerable<LocalFile> getLocalFiles(IEnumerable<DirectorySetup> dirSetups, IEnumerable<string> fileExtensions)
            => dirSetups.SelectMany(setup => getLocalFiles(setup, fileExtensions));

        private static IEnumerable<string> getDistinctPaths(IEnumerable<LocalFile> localFiles, IEnumerable<FileInfoEx> definedFiles)
        {
            var localPaths = localFiles.Select(file => file.FileInfo.FullName);
            var definedPaths = definedFiles.Select(file => file.AbsolutePath);
            return localPaths.Concat(definedPaths).Distinct();
        }

        private IEnumerable<DirectorySetup> getDirSetups()
            => this.directoryDataService.Get().KeyValues.Select(x => x.Value);


        #endregion



        #region hashing utils
        public FileInfo? UpdateFile(DirectorySetup dirSetup, Uri relativePath)
        {
            var localPath = new Uri(dirSetup.Path, relativePath).LocalPath;
            var fileInfo = new SysFileInfo(localPath);
            return this.modelFileDataService.Patch(
            new FileInfoChangeset()
            {
                Path = relativePath,
                CreationTime = fileInfo.CreationTime,
                FileHash = fileManagementService.HashFile(localPath),
                DirectorySetupId = dirSetup.Id,
                FileSize = fileInfo.Length,
                IsAccessible = File.Exists(localPath),
                LastWriteTime = fileInfo.LastWriteTime
            });
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
        private class FileUpdateInfo
        {
            FileInfoEx? virtualFile { get; set; }
            SysFileInfo actualFile { get; set; }
            FileHash? updatedHash { get; set; }
        }
    }
}
