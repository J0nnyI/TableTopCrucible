using Microsoft.WindowsAPICodePack.Shell;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;

using TableTopCrucible.Core.Enums;
using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Data.Files.Scanner;
using TableTopCrucible.Data.Models.Sources;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Library.Exceptions;
using TableTopCrucible.Domain.Models.ValueTypes;

using FileInfo = TableTopCrucible.Data.Models.Sources.FileInfo;
using SysFileInfo = System.IO.FileInfo;

namespace TableTopCrucible.Domain.Library
{
    public interface IThumbnailManagementService
    {
        IEnumerable<ITaskProgressionInfo> CreateAndLinkThumbnail();
        void LinkThumbnail(ItemEx item, string thumbnailPath, out FileInfo file, out FileItemLinkChangeset linkCs, FileItemLink? linkSource = null);
        void CreateAndLinkThumbnail(ItemEx item, Action<FileStream> streamWriter, out FileInfo file, out FileItemLinkChangeset linkCs, FileItemLink? linkSource = null);
        void CreateAndLinkThumbnail(ItemEx item);
        void CreateAndLinkThumbnail(IEnumerable<ItemEx> items);

    }
    public class ThumbnailManagementService : IThumbnailManagementService
    {
        private readonly IItemDataService itemService;
        private readonly ISettingsService settingsService;
        private readonly IFileItemLinkService fileItemLinkService;
        private readonly IModelFileDataService modelFileDataService;
        private readonly IFileManagementService fileManagement;
        private readonly IImageFileDataService _imageFileDataService;

        public ThumbnailManagementService(
            IItemDataService itemService,
            ISettingsService settingsService,
            IFileItemLinkService fileItemLinkService,
            IModelFileDataService modelFileDataService,
            IFileManagementService fileManagement,
            IImageFileDataService imageFileDataService)
        {
            this.itemService = itemService;
            this.settingsService = settingsService;
            this.fileItemLinkService = fileItemLinkService;
            this.modelFileDataService = modelFileDataService;
            this.fileManagement = fileManagement;
            _imageFileDataService = imageFileDataService;
        }

        private void createThumbnail(string sourcePath, FileStream fs)
        {
            using ShellFile shellFile = ShellFile.FromFilePath(sourcePath);
            using Bitmap shellThumb = shellFile.Thumbnail.ExtraLargeBitmap;
            shellThumb.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
        }
        private void createThumbnailDir(string thumbnailDir)
        {
            var rootDir = Path.GetDirectoryName(thumbnailDir);
            if (!Directory.Exists(rootDir))
                Directory.CreateDirectory(rootDir);
        }
        public void CreateAndLinkThumbnail(ItemEx item, Action<FileStream> streamWriter, out FileInfo file, out FileItemLinkChangeset linkCs, FileItemLink? linkSource = null)
        {
            var relativeThumbnailPath = item.GenerateRelativeThumbnailPath();
            var root = item.RootPath;
            var absolutePath = Path.Combine(root, relativeThumbnailPath);

            createThumbnailDir(absolutePath);

            using (var fs = File.OpenWrite(absolutePath))
            {
                streamWriter(fs);
                fs.Close();
            }

            LinkThumbnail(item, absolutePath, out file, out linkCs, linkSource);
        }

        public void CreateAndLinkThumbnail(ItemEx item)
        {
            try
            {
                var imgPath = item.GenerateAbsoluteThumbnailPath();
                var itemPath = item.LatestFilePath;

                this.CreateAndLinkThumbnail(item, fs =>
                {
                    createThumbnail(itemPath, fs);
                }, out var file, out var linkCs);

                fileItemLinkService.Patch(linkCs);
            }
            catch (Exception ex)
            {
                throw new ThumbnailException("Thumbnail Could not be created", ex);
            }
        }

        public void LinkThumbnail(ItemEx item, string thumbnailPath, out FileInfo file, out FileItemLinkChangeset linkCs, FileItemLink? linkSource = null)
        {
            FileHash hash = FileHash.Create(thumbnailPath);

            file = new FileInfoChangeset(item.DirectorySetups.FirstOrDefault(), new SysFileInfo(thumbnailPath), hash).ToEntity();

            var existing = modelFileDataService.GetExtendedByHash().Lookup(file.HashKey.Value);
            if (existing.HasValue)
                file = existing.Value.FileInfo;

            linkCs = new FileItemLinkChangeset(linkSource ?? item.LatestVersionedFile.Value.Link.Link)
            {
                ThumbnailKey = file.HashKey
            };

            this.modelFileDataService.Post(file);
        }

        public IEnumerable<ITaskProgressionInfo> CreateAndLinkThumbnail()
        {
            return this.itemService
                .GetExtended()
                .Items
                .Where(item =>
                    !item.LatestThumbnail.HasValue
                    && item.LatestVersionedFile != null
                    )
                .SplitEvenly(settingsService.ThreadCount)
                .Select(chunk =>
                {
                    TaskProgression prog = new TaskProgression();
                    prog.Title = "Creating Thumbnails";
                    prog.RequiredProgress = chunk.Count();
                    Observable.Start(() =>
                    {
                        prog.State = TaskState.InProgress;
                        var chunkResult = chunk.Select(item =>
                        {
                            FileInfo file = default;
                            FileItemLinkChangeset linkCs = null;
                            FailedThumbnailResult? err = null;

                            var imgPath = item.GenerateAbsoluteThumbnailPath();
                            var itemPath = item.LatestFilePath;
                            try
                            {



                                this.CreateAndLinkThumbnail(item, fs =>
                                {
                                    createThumbnail(itemPath, fs);
                                }, out file, out linkCs);
                            }
                            catch (Exception ex)
                            {
                                err = new FailedThumbnailResult(ex, itemPath, imgPath);
                                prog.State = TaskState.RunningWithErrors;
                            }
                            prog.CurrentProgress++;
                            return new { file, linkCs, err };
                        }).ToArray();


                        var groupedResult = chunkResult.GroupBy(data => data.err.HasValue);
                        var successes = groupedResult.FirstOrDefault(x => !x.Key)?.Where(x => x != null);
                        var errors = groupedResult.FirstOrDefault(x => x.Key)?.Select(x => x.err.Value);

                        if (prog.State == TaskState.RunningWithErrors)
                            prog.State = successes?.Any() == true ? TaskState.PartialSuccess : TaskState.Failed;
                        else
                            prog.State = TaskState.Done;

                        if (errors?.Any() == true)
                        {
                            var eGroups = errors.GroupBy(e => e.Exception.GetType().FullName);
                            string msg = "the following Thumbnails could not be created";

                            foreach (var eGroup in eGroups)
                            {
                                msg += string.Join(Environment.NewLine, new string[]{
                                    "",
                                    "--------------------------------",
                                    eGroup.Key,
                                    "3dFile / thumbnail",
                                    string.Join(Environment.NewLine, eGroup.Select(e => $"{e.FilePath} / {e.ThumbnailPath}"))
                                });
                            }
                            prog.Error = new Exception(msg);
                        }
                    }, RxApp.TaskpoolScheduler);
                    return prog as ITaskProgressionInfo;
                })
                .ToArray();
        }
        private struct FailedThumbnailResult
        {
            public FailedThumbnailResult(Exception exception, string filePath, string thumbnailPath)
            {
                Exception = exception;
                FilePath = filePath;
                ThumbnailPath = thumbnailPath;
            }

            public Exception Exception { get; }
            public string FilePath { get; }
            public string ThumbnailPath { get; }
        }

        #region rework

        private struct ThumbnailGenerationInfo
        {
            public ItemEx Item { get; set; }
            public string ThumbnailPath { get; set; }
            public FileHash FileHash { get; set; }
            public FileInfo? FileInfo { get; set; }
            public FileItemLinkChangeset FileItemLinkChangeset { get; set; }
            public Exception Error { get; set; }
        }

        public void CreateAndLinkThumbnail(IEnumerable<ItemEx> items)
        {
            Observable.Start(() =>
            {
                items
                .Where(item => item.LatestFile.HasValue)
                .SelectAsync((ItemEx rawItem) =>
                {
                    var item = new ThumbnailGenerationInfo { Item = rawItem };
                    try
                    {
                        item.ThumbnailPath = item.Item.GenerateAbsoluteThumbnailPath();
                        createThumbnailDir(item.ThumbnailPath);
                        using (FileStream fs = File.Create(item.ThumbnailPath))
                        {
                            createThumbnail(item.Item.LatestFilePath, fs);
                        }
                        item.FileHash = FileHash.Create(item.ThumbnailPath, SHA512.Create());

                        item.FileInfo = new FileInfoChangeset(item.Item.LatestFile.Value.DirectorySetup, new SysFileInfo(item.ThumbnailPath), item.FileHash).ToEntity();
                        item.FileItemLinkChangeset = new FileItemLinkChangeset(item.Item.LatestVersionedFile.Value.Link.Link) { ThumbnailKey = item.FileInfo.Value.HashKey };
                    }
                    catch (Exception ex)
                    {
                        item.Error = ex;
                    }
                    return item;
                }, settingsService.MaxPatchSize, out var progress)
                .CombineLatest()
                .Select(groups => groups.SelectMany(group => group).Where(item=>item.Error == null))
                .Subscribe(items =>
                {
                    this._imageFileDataService.Post(items.Select(file => file.FileInfo.Value));
                    this.fileItemLinkService.Patch(items.Select(file => file.FileItemLinkChangeset));
                });
            }, RxApp.TaskpoolScheduler);
        }

        #endregion

    }
}
