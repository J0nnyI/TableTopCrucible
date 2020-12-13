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

namespace TableTopCrucible.Domain.Library
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
            FileManagementService.SupportedModelTypes.Contains(FileExtension)
                ? FileType.Model :
            FileManagementService.SupportedImageTypes.Contains(FileExtension)
                ? FileType.Image :
                FileType.Unknown;
        public bool IsNew => this.LocalFile.HasValue && !this.VirtualFile.HasValue;
        public bool IsDeleted => !this.LocalFile.HasValue && this.VirtualFile.HasValue;
        public bool IsUpdated => this.LocalFile?.FileInfo.LastWriteTime != this.VirtualFile?.LastWriteTime && this.LocalFile.HasValue && this.VirtualFile.HasValue;
        public bool IsUnchanged => this.LocalFile?.FileInfo.LastWriteTime == this.VirtualFile?.LastWriteTime && this.LocalFile.HasValue && this.VirtualFile.HasValue;
        public bool RequiresHashUpdate => (IsNew || IsUpdated) && !UpdatedHash.HasValue;
        public void UpdateHash(HashAlgorithm hashAlgorithm)
        {
            if (!RequiresHashUpdate)
                return;
            string path = LocalFile.Value.FileInfo.FullName;

            if (!File.Exists(path))
                throw new FileNotFoundException($"could not find file {path}");

            using FileStream stream = File.OpenRead(path);
            byte[] data = hashAlgorithm.ComputeHash(stream);
            UpdatedHash = FileHash.Create(path, hashAlgorithm);

        }
        public void UpdateHash()
        {
            using HashAlgorithm hashAlgorithm = SHA512.Create();
            UpdateHash(hashAlgorithm);
        }
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
        ITaskProgressionInfo HashingProgress { get; }
        ITaskProgressionInfo TotalProgress { get; }
        IObservable<bool> IsSynchronizingChanges { get; }
        bool IsSynchronizing { get; }

        public void StartSynchronization();
        FileInfo? UpdateFile(DirectorySetup dirSetup, Uri relativePath, IFileDataService fileService);
    }

    public class FileManagementService : IFileManagementService
    {
        public static readonly IEnumerable<string> SupportedModelTypes = new string[] { "stl", "obj", "off", "objz", "lwo", "3ds" };
        public static readonly IEnumerable<string> SupportedImageTypes = new string[] { "png", "jpg", "jpeg", "bmp", "gif", "hdp", "jp2", "pbm", "psd", "tga", "tiff", "img" };
        public static readonly IEnumerable<string> SupportedArchiveTypes = new string[] { };
        private readonly IImageFileDataService _imageFileDataService;
        private readonly IModelFileDataService _modelFileDataService;
        private readonly IDirectoryDataService _directoryDataService;
        private readonly ISettingsService _settingsService;

        [Reactive] public ITaskProgressionInfo HashingProgress { get; private set; }
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

            IsSynchronizingChanges = this.WhenAnyValue(vm => vm.HashingProgress, vm => vm.TotalProgress, (hash, total) => hash != null || total != null);
            IsSynchronizingChanges.Subscribe(isSyncing => this.IsSynchronizing = isSyncing);


        }
        public void StartSynchronization()
        {
            if (HashingProgress != null || TotalProgress != null)
                throw new InvalidOperationException("hashing is already in progress");
            var prog = new TaskProgression()
            {
                RequiredProgress = 6,
                Title = "Synchronization Progress"
            };
            this.TotalProgress = prog;
            prog.Details = "reading files";
            prog.DoneChanges.Subscribe(_=>this.TotalProgress = null);
            prog.DoneChanges.Subscribe(_=>this.TotalProgress = null);

            _getFiles(
                _modelFileDataService,
                SupportedModelTypes.Concat(SupportedImageTypes),
                out var deletedFiles,
                out var updatedFiles,
                out var newFiles);
            prog.CurrentProgress++;
            prog.Details = "Hashing";

            using HashAlgorithm hashAlgorithm = SHA512.Create();

            HashingProgress = newFiles
                .Concat(updatedFiles)
                .Where(file => file.RequiresHashUpdate)
                .ForEachAsync(file => file.UpdateHash(), _settingsService.ThreadCount, SplitMode.Threadcount);
            HashingProgress.Title = "Hashing";
            HashingProgress.DoneChanges.Subscribe(res =>
            {
                this.HashingProgress = null;
                try
                {
                    if (res == TaskState.Done)
                    {
                        prog.Details = "detecting deleted files";
                        var deleteProgs = _handleDeletedFiles(deletedFiles);
                        deleteProgs.OnAllComplete.Subscribe(_ =>
                        {

                            prog.Details = "detecting new files";
                            var newFileProgs = _handleNewFiles(newFiles);
                            newFileProgs.OnAllComplete.Subscribe(_ =>
                            {
                                prog.Details = "detecting updated files";
                                var updatedProgs = _handleUpdatedFiles(updatedFiles);
                                updatedProgs.OnAllComplete.Subscribe(_ =>
                                {
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
                )));
        }
        private DualPatchResult _handleNewFiles(IEnumerable<FileUpdateInfo> files)
        {
            return _handleMultiCacheAction(files, (cache, gFiles) => cache.Patch(
                gFiles.Select(gFile => new FileInfoChangeset(
                    gFile.LocalFile.Value.DirectorySetup,
                    gFile.LocalFile.Value.FileInfo,
                    gFile.UpdatedHash.Value)
                )));
        }
        private DualPatchResult _handleDeletedFiles(IEnumerable<FileUpdateInfo> files)
        {
            return _handleMultiCacheAction(files, (service, gFiles)
                => service.Delete(files.Select(file => file.VirtualFile.Value.Id)));
        }
        private DualPatchResult _handleMultiCacheAction(IEnumerable<FileUpdateInfo> files, Action<IFileDataService, IEnumerable<FileUpdateInfo>> action)
        {
            var res = new DualPatchResult();
            var groupedFiles = files.GroupBy(file => file.FileType);


            res.ModelProgress = groupedFiles
                .FirstOrDefault(groupedFiles => groupedFiles.Key == FileType.Model)
                ?.ForEachAsync(gFiles => action(_modelFileDataService, gFiles), _settingsService.MaxPatchSize, SplitMode.Patchsize);

            if (res.ModelProgress == null)
                res.ModelProgress = new TaskProgression()
                {
                    Title = "Model Job",
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
                        Title = "Image Job",
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
            var dirSetups = this._directoryDataService.Get().Items;
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
                                LocalFile = mLocalFile.Any() ? (mLocalFile.First() as LocalFile?) : null,
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
            public DualPatchResult()
            {
                this.OnAllComplete = this.WhenAnyObservable(vm => vm.ImageProgress.DoneChanges, vm => vm.ModelProgress.DoneChanges, (image, model) => new DualPatchResultState(image, model));
            }
        }
    }
}
