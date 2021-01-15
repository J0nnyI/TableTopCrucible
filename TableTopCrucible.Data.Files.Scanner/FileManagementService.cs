using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
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
using TableTopCrucible.Data.Models.Sources;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.ValueTypes;

using FileInfo = TableTopCrucible.Data.Models.Sources.FileInfo;
using SysFileInfo = System.IO.FileInfo;
using Version = TableTopCrucible.Domain.Models.ValueTypes.Version;

namespace TableTopCrucible.Data.Files.Scanner
{
    internal enum FileType
    {
        Model,
        Image,
        Archive,
        Unknown
    }
    internal class FileUpdateInfo
    {
        public FileInfoEx? VirtualFile { get; set; }
        public LocalFile? LocalFile { get; set; }
        public string FileExtension =>
            LocalFile.HasValue
                ? Path.GetExtension(LocalFile.Value.FileInfo.FullName)
                : Path.GetExtension(VirtualFile.Value.AbsolutePath);
        public FileHash? UpdatedHash { get; set; }
        public FileType FileType =>
            FileSupportHelper.ModelExtensions.Contains(FileExtension)
                ? FileType.Model :
            FileSupportHelper.ImageExtensions.Contains(FileExtension)
                ? FileType.Image :
                FileType.Unknown;
        public bool IsNew => LocalFile.HasValue && !VirtualFile.HasValue;
        public bool IsDeleted => !LocalFile.HasValue && VirtualFile.HasValue;
        public bool IsUpdated => LocalFile?.FileInfo.LastWriteTime != VirtualFile?.LastWriteTime && LocalFile.HasValue && VirtualFile.HasValue;
        public bool IsUnchanged => LocalFile?.FileInfo.LastWriteTime == VirtualFile?.LastWriteTime && LocalFile.HasValue && VirtualFile.HasValue;
        public bool RequiresHashUpdate => (IsNew || IsUpdated) && !UpdatedHash.HasValue;

    }
    internal struct LocalFile
    {
        public LocalFile(DirectorySetup directorySetup, string localPath)
        {
            DirectorySetup = directorySetup;
            FileInfo = new SysFileInfo(localPath);
        }

        public DirectorySetup DirectorySetup { get; }
        public SysFileInfo FileInfo { get; }
    }
    public interface IFileManagementService
    {
        ITaskProgressionInfo SubProgress { get; }
        ITaskProgressionInfo TotalProgress { get; }
        IObservable<bool> IsSynchronizingChanges { get; }
        bool IsSynchronizing { get; }

        public void StartSynchronization();
        FileInfo? UpdateFile(DirectorySetup dirSetup, Uri relativePath, IFileDataService fileService);
    }

    public class FileManagementService : DisposableReactiveObjectBase, IFileManagementService
    {
        public static readonly IEnumerable<string> SupportedArchiveTypes = new string[] { };
        private readonly IImageFileDataService _imageFileDataService;
        private readonly IModelFileDataService _modelFileDataService;
        private readonly IDirectoryDataService _directoryDataService;
        private readonly ISettingsService _settingsService;

        [Reactive] public ITaskProgressionInfo SubProgress { get; private set; }
        [Reactive] public ITaskProgressionInfo TotalProgress { get; private set; }
        public IObservable<bool> IsSynchronizingChanges { get; }
        [Reactive] public bool IsSynchronizing { get; private set; } = false;

        public FileManagementService(
            IImageFileDataService imageFileDataService,
            IModelFileDataService modelFileDataService,
            IDirectoryDataService directoryDataService,
            ISettingsService settingsService)
        {
            _imageFileDataService = imageFileDataService;
            _modelFileDataService = modelFileDataService;
            _directoryDataService = directoryDataService;
            _settingsService = settingsService;

            IsSynchronizingChanges = this.WhenAnyValue(vm => vm.SubProgress, vm => vm.TotalProgress, (hash, total) => hash != null || total != null && !total.State.IsIn(TaskState.Done, TaskState.PartialSuccess, TaskState.Failed));
            IsSynchronizingChanges.Subscribe(isSyncing => IsSynchronizing = isSyncing);

        }
        public void StartSynchronization()
        {
            if (IsSynchronizing)
                throw new InvalidOperationException("file synchronization is already in progress");
            var prog = new TaskProgression()
            {
                RequiredProgress = 9,
                Title = "Synchronization Progress"
            };
            TotalProgress = prog;
            prog.Details = "reading files";

            _getFiles(
                _modelFileDataService,
                FileSupportHelper.ModelExtensions.Concat(FileSupportHelper.ImageExtensions),
                out var deletedFiles,
                out var updatedFiles,
                out var newFiles);
            prog.CurrentProgress++;
            prog.Details = "Hashing";

            SubProgress = FileHash.CreateMany(
                newFiles
                    .Concat(updatedFiles)
                    .Where(x=>x.RequiresHashUpdate),
                model=>model.LocalFile.Value.FileInfo.FullName,
                (model, hash)=>model.UpdatedHash = hash,
                _settingsService.ThreadCount);

            SubProgress.DoneChanges.Subscribe(res =>
            {
                SubProgress = null;
                prog.CurrentProgress++;
                try
                {
                    if (res == TaskState.Done)
                    {
                        prog.Details = "detecting deleted files";
                        var deleteProgs = _handleDeletedFiles(deletedFiles);

                        deleteProgs.ImageProgressChanges.Subscribe(subProg => { SubProgress = subProg; prog.CurrentProgress++; });
                        deleteProgs.ModelProgressChanges.Subscribe(subProg => { SubProgress = subProg; prog.CurrentProgress++; });

                        deleteProgs.OnAllComplete.Subscribe(_ =>
                        {
                            prog.CurrentProgress++;

                            prog.Details = "detecting new files";
                            var newFileProgs = _handleNewFiles(newFiles);

                            newFileProgs.ImageProgressChanges.Subscribe(subProg => { SubProgress = subProg; prog.CurrentProgress++; });
                            newFileProgs.ModelProgressChanges.Subscribe(subProg => { SubProgress = subProg; prog.CurrentProgress++; });

                            newFileProgs.OnAllComplete.Subscribe(_ =>
                            {
                                prog.CurrentProgress++;
                                prog.Details = "detecting updated files";
                                var updatedProgs = _handleUpdatedFiles(updatedFiles);

                                updatedProgs.ImageProgressChanges.Subscribe(subProg => { SubProgress = subProg; prog.CurrentProgress++; });
                                updatedProgs.ModelProgressChanges.Subscribe(subProg => { SubProgress = subProg; prog.CurrentProgress++; });

                                updatedProgs.OnAllComplete.Subscribe(_ =>
                                {
                                    SubProgress = null;
                                    prog.CurrentProgress++;
                                    prog.State = TaskState.Done;
                                    prog.Details = "done";
                                });
                            });
                        });
                    }
                    else
                    {
                        prog.State = res;
                        prog.Details = "hashing failed";
                    }
                }
                catch (Exception ex)
                {
                    prog.State = TaskState.Failed;
                    prog.Details = ex.Message;
                }
            });


        }
        private DualPatchResult _handleUpdatedFiles(IEnumerable<FileUpdateInfo> files)
        {
            return _handleMultiCacheAction(files, (cache, gFiles) => cache.Patch(
                gFiles.Select(gFile => new FileInfoChangeset(
                    gFile.LocalFile.Value.DirectorySetup,
                    gFile.LocalFile.Value.FileInfo,
                    gFile.UpdatedHash.Value,
                    gFile.VirtualFile.Value.FileInfo)
                )), "Updating Models", "Updating Images");
        }
        private DualPatchResult _handleNewFiles(IEnumerable<FileUpdateInfo> files)
        {
            return _handleMultiCacheAction(files, (cache, gFiles) => cache.Patch(
                gFiles.Select(gFile => new FileInfoChangeset(
                    gFile.LocalFile.Value.DirectorySetup,
                    gFile.LocalFile.Value.FileInfo,
                    gFile.UpdatedHash.Value)
                )), "adding new models", "adding new images");
        }
        private DualPatchResult _handleDeletedFiles(IEnumerable<FileUpdateInfo> files)
        {
            return _handleMultiCacheAction(files, (service, gFiles)
                => service.Delete(files.Select(file => file.VirtualFile.Value.Id)), "removing deleted models", "removing deleted images");
        }
        private DualPatchResult _handleMultiCacheAction(IEnumerable<FileUpdateInfo> files, Action<IFileDataService, IEnumerable<FileUpdateInfo>> action, string modelJobTitle = "ModelJob", string imageJobTitle = "Image Job")
        {
            var res = new DualPatchResult();
            var groupedFiles = files.GroupBy(file => file.FileType);


            res.ModelProgress = groupedFiles
                .FirstOrDefault(groupedFiles => groupedFiles.Key == FileType.Model)
                ?.ForEachAsync(gFiles => action(_modelFileDataService, gFiles), _settingsService.MaxPatchSize, SplitMode.Patchsize);

            if (res.ModelProgress == null)
                res.ModelProgress = new TaskProgression()
                {
                    Title = modelJobTitle,
                    RequiredProgress = 0,
                    CurrentProgress = 0,
                    State = TaskState.Done
                };

            res.ModelProgress.DoneChanges.Subscribe(_ =>
            {
                res.ImageProgress = groupedFiles
                    .FirstOrDefault(groupedFiles => groupedFiles.Key == FileType.Image)
                    ?.ForEachAsync(gFiles => action(_imageFileDataService, gFiles), _settingsService.MaxPatchSize, SplitMode.Patchsize);
                if (res.ImageProgress == null)
                    res.ImageProgress = new TaskProgression()
                    {
                        Title = imageJobTitle,
                        RequiredProgress = 0,
                        CurrentProgress = 0,
                        State = TaskState.Done
                    };
            });
            return res;
        }

        private void _getFiles(
            IFileDataService fileService,
            IEnumerable<string> fileExtensions,
            out IEnumerable<FileUpdateInfo> deletedFiles,
            out IEnumerable<FileUpdateInfo> updatedFiles,
            out IEnumerable<FileUpdateInfo> newFiles)
        {
            var dirSetups = _directoryDataService.Get().Items;
            var localFiles = _getLocalFiles(dirSetups, fileExtensions);
            var definedFiles = _getDefinedFiles(dirSetups, fileService);
            var paths = _getDistinctPaths(localFiles, definedFiles);

            var allFiles = (from file in paths
                            join localFile in localFiles
                                on file.ToLower() equals localFile.FileInfo.FullName.ToLower() into mLocalFile
                            join definedFile in definedFiles
                                on file.ToLower() equals definedFile.AbsolutePath.ToLower() into mDefinedFile
                            select new FileUpdateInfo
                            {
                                LocalFile = mLocalFile.Any() ? mLocalFile.First() as LocalFile? : null,
                                VirtualFile = mDefinedFile.Any() ? mDefinedFile.First() as FileInfoEx? : null
                            }).ToArray();

            deletedFiles = allFiles.Where(file => file.IsDeleted);
            updatedFiles = allFiles.Where(file => file.IsUpdated);
            newFiles = allFiles.Where(file => file.IsNew);
        }

        private static IEnumerable<FileInfoEx> _getDefinedFiles(IEnumerable<DirectorySetup> dirSetups, IFileDataService dataService)
        {
            var dirIDs = dirSetups.Select(x => x.Id);
            return dataService.GetExtended()
                .KeyValues
                .Select(x => x.Value)
                .Where(file => dirIDs
                .Contains(file.DirectorySetup.Id));
        }
        private static IEnumerable<LocalFile> _getLocalFiles(DirectorySetup directorySetup, IEnumerable<string> fileExtensions)
        {
            return Directory
                .EnumerateFiles(directorySetup.Path.LocalPath, "*", SearchOption.AllDirectories)
                .Where(file => fileExtensions.Contains(Path.GetExtension(file)))
                .Select(file => new LocalFile(directorySetup, file));
        }
        private static IEnumerable<LocalFile> _getLocalFiles(IEnumerable<DirectorySetup> dirSetups, IEnumerable<string> fileExtensions)
            => dirSetups.SelectMany(setup => _getLocalFiles(setup, fileExtensions));

        private static IEnumerable<string> _getDistinctPaths(IEnumerable<LocalFile> localFiles, IEnumerable<FileInfoEx> definedFiles)
        {
            var localPaths = localFiles.Select(file => file.FileInfo.FullName);
            var definedPaths = definedFiles.Select(file => file.AbsolutePath);
            return localPaths.Concat(definedPaths).Distinct();
        }

        public FileInfo? UpdateFile(DirectorySetup dirSetup, Uri relativePath, IFileDataService fileService)
            => fileService.Patch(new FileInfoChangeset(dirSetup, relativePath));
        private class DualPatchResult : ReactiveObject
        {
            public struct DualPatchResultState
            {
                public DualPatchResultState(TaskState imageResult, TaskState modelResult)
                {
                    ModelResult = modelResult;
                    ImageResult = imageResult;

                }

                public TaskState ModelResult { get; }
                public TaskState ImageResult { get; }

            }



            [Reactive]
            public ITaskProgressionInfo ImageProgress { get; set; }
            [Reactive]
            public ITaskProgressionInfo ModelProgress { get; set; }
            public IObservable<DualPatchResultState> OnAllComplete { get; }
            public IObservable<ITaskProgressionInfo> ImageProgressChanges { get; }
            public IObservable<ITaskProgressionInfo> ModelProgressChanges { get; }
            public DualPatchResult()
            {
                OnAllComplete = this.WhenAnyObservable(vm => vm.ImageProgress.DoneChanges, vm => vm.ModelProgress.DoneChanges, (image, model) => new DualPatchResultState(image, model));
                ImageProgressChanges = this.WhenAnyValue(vm => vm.ImageProgress);
                ModelProgressChanges = this.WhenAnyValue(vm => vm.ModelProgress);
            }
        }
    }
}
