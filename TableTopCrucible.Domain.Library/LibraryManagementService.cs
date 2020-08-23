﻿using DynamicData;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Enums;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using TableTopCrucible.WPF.Helper;

using FileInfo = TableTopCrucible.Domain.Models.Sources.FileInfo;
using SysFileInfo = System.IO.FileInfo;
using Version = TableTopCrucible.Domain.Models.ValueTypes.Version;
namespace TableTopCrucible.Domain.Library

{
    public interface ILibraryManagementService
    {
        public void FullSync();
        public void FullSync(IEnumerable<DirectorySetup> dirSetups);
        void AutoGenerateItems();
        FileInfo? UpdateFile(DirectorySetup dirSetup, Uri relativePath);
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


        public Version GetNextVersion(Version previousVersion, IEnumerable<Version> takenVersions)
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

        public void FullSync()
            => FullSync(getDirSetups());
        public async void FullSync(IEnumerable<DirectorySetup> dirSetups)
        {
            try
            {

                var localFiles = getLocalFiles(dirSetups);
                var definedFiles = getDefinedFiles(dirSetups);

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

                /**
                 * await does not work
                 */
                var FilesToHash = allFiles
                    .Where(x => x.isNew || x.isUpdated);

                if (FilesToHash.Any())
                {

                    var hashedFiles = (await FilesToHash
                        .SplitEvenly(settingsService.MaxPatchSize)
                        .Select(chunk =>
                        {
                            return Observable.Start(() =>
                            {
                                var hasher = SHA512.Create();

                                return chunk.Select(file =>
                                {
                                    return new
                                    {
                                        isUpdate = file.definedFile.HasValue,
                                        file.localFile.Value.DirectorySetup,
                                        file.localFile.Value.FileInfo,
                                        hash = _hashFile(hasher, file.localFile.Value.FileInfo.FullName)
                                    };
                                });
                            }, RxApp.TaskpoolScheduler);
                        })
                        .CombineLatest())
                        .SelectMany(x => x)
                        .GroupBy(x => x.isUpdate);

                    var filesToUpdate = hashedFiles
                        .FirstOrDefault(x => x.Key == true)
                        ?.Select(x => new { x.DirectorySetup, x.FileInfo, x.hash, x.isUpdate, hashKey = new FileInfoHashKey(x.hash, x.FileInfo.Length) })
                        ?.Join(
                            allFiles.Where(x => x.isUpdated),
                            x => x.FileInfo.FullName,
                            x => x.localFile?.FileInfo?.FullName,
                            (localFile, definedFile) => new
                            {
                                newHash = localFile.hashKey,
                                newFileInfo = localFile.FileInfo,
                                definedFile = definedFile.definedFile.Value
                            })
                        ?.GroupBy(x => x.newHash);

                    var fileUpdateChangesets = new List<FileInfoChangeset>();
                    var fileItemLinkChangesets = new List<FileItemLinkChangeset>();
                    if (filesToUpdate != null)
                    {
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

                        this.fileDataService.Patch(fileUpdateChangesets);
                        this.fileItemLinkService.Patch(fileItemLinkChangesets);
                    }

                    var newFiles = hashedFiles
                        .FirstOrDefault(x => x.Key == false)
                        ?.Select(file =>
                        {
                            var changeset = new FileInfoChangeset();
                            changeset.SetSysFileInfo(file.DirectorySetup, file.FileInfo);
                            changeset.FileHash = file.hash;
                            changeset.DirectorySetupId = file.DirectorySetup.Id;
                            changeset.Id = FileInfoId.New();
                            return changeset;
                        })
                        .ToArray();
                    if (newFiles != null)
                        this.fileDataService.Patch(newFiles);

                }


                // stage 3: remove deleted Files

                this.fileDataService.Delete(allFiles.Where(x => x.isDeleted).Select(x => x.definedFile.Value.Id));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }



        #region file sync utils

        private IEnumerable<FileInfoEx> getDefinedFiles(IEnumerable<DirectorySetup> dirSetups)
        {
            var dirIDs = dirSetups.Select(x => x.Id);
            return fileDataService.GetExtended()
                .KeyValues
                .Select(x => x.Value)
                .Where(file => dirIDs
                .Contains(file.DirectorySetup.Id));
        }


        private IEnumerable<LocalFile> getLocalFiles(DirectorySetup directorySetup)
        {
            return Directory.GetFiles(directorySetup.Path.LocalPath, "*", SearchOption.AllDirectories)
                .Select(file => new LocalFile(directorySetup, file));
        }

        private IEnumerable<LocalFile> getLocalFiles(IEnumerable<DirectorySetup> dirSetups)
            => dirSetups.SelectMany(setup => getLocalFiles(setup));

        private IEnumerable<string> getDistinctPaths(IEnumerable<LocalFile> localFiles, IEnumerable<FileInfoEx> definedFiles)
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
            return this.fileDataService.Patch(
            new FileInfoChangeset()
            {
                Path = relativePath,
                CreationTime = fileInfo.CreationTime,
                FileHash = _hashFile(localPath),
                DirectorySetupId = dirSetup.Id,
                FileSize = fileInfo.Length,
                IsAccessible = File.Exists(localPath),
                LastWriteTime = fileInfo.LastWriteTime
            });
        }

        private FileHash _hashFile(string path)
        {
            using (HashAlgorithm hashAlgorithm = SHA512.Create())
                return _hashFile(hashAlgorithm, path);
        }

        private FileHash _hashFile(HashAlgorithm hashAlgorithm, string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"could not find file {path}");

            FileHash result;

            using (FileStream stream = File.OpenRead(path))
            {
                byte[] data = hashAlgorithm.ComputeHash(stream);
                result = new FileHash(data);
            }


            return result;
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
