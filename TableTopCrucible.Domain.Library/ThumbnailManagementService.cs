using Microsoft.WindowsAPICodePack.Shell;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using TableTopCrucible.Core.Enums;
using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.Sources;

using FileInfo = TableTopCrucible.Domain.Models.Sources.FileInfo;
using SysFileInfo = System.IO.FileInfo;

namespace TableTopCrucible.Domain.Library
{
    public interface IThumbnailManagementService
    {
        IEnumerable<ITaskProgressionInfo> GenerateAll();

    }
    public class ThumbnailManagementService:IThumbnailManagementService
    {
        private readonly IItemService itemService;
        private readonly ISettingsService settingsService;
        private readonly IFileItemLinkService fileItemLinkService;
        private readonly IFileDataService fileDataService;
        private readonly IFileManagementService fileManagement;

        public ThumbnailManagementService(
            IItemService itemService,
            ISettingsService settingsService,
            IFileItemLinkService fileItemLinkService,
            IFileDataService fileDataService,
            IFileManagementService fileManagement)
        {
            this.itemService = itemService;
            this.settingsService = settingsService;
            this.fileItemLinkService = fileItemLinkService;
            this.fileDataService = fileDataService;
            this.fileManagement = fileManagement;
        }
        public IEnumerable<ITaskProgressionInfo> GenerateAll()
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
                                prog.Details = itemPath;
                                if (!Directory.Exists(Path.GetFullPath(imgPath)))
                                    Directory.CreateDirectory(Path.GetDirectoryName(imgPath));

                                using ShellFile shellFile = ShellFile.FromFilePath(itemPath);
                                using Bitmap shellThumb = shellFile.Thumbnail.ExtraLargeBitmap;
                                using (Stream fs = File.OpenWrite(imgPath))
                                {
                                    shellThumb.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                                }

                                file = new FileInfoChangeset(item.DirectorySetups.FirstOrDefault(), new SysFileInfo(imgPath), fileManagement.HashFile(imgPath)).ToEntity();

                                linkCs = new FileItemLinkChangeset(item.LatestVersionedFile.Value.Link.Link)
                                {
                                    ThumbnailKey = file.HashKey
                                };

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
                        if (successes?.Any() == true)
                        {
                            fileItemLinkService.Patch(successes.Select(x => x.linkCs));
                            fileDataService.Post(successes.Select(x => x.file).Where(x => x != null));
                        }

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
    }
}
