using DynamicData;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;

using TableTopCrucible.Base.Enums;
using TableTopCrucible.Base.Models.Sources;
using TableTopCrucible.Base.ValueTypes;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using TableTopCrucible.Domain.Models.Views;

using FileInfo = TableTopCrucible.Domain.Models.Sources.FileInfo;
using SysFileInfo = System.IO.FileInfo;

namespace TableTopCrucible.Domain.Services
{
    public interface IFileInfoService : IDataService<FileInfo, FileInfoId, FileInfoChangeset>
    {
        void Synchronize();
        void UpdateHashes();
        void UpdateHashes(int threadcount);
        public IObservableCache<ExtendedFileInfo, FileInfoId> GetExtended();
        public IObservableCache<FileInfo, FileInfoId> Get(DirectorySetupId directorySetupId);
        public IObservableCache<ExtendedFileInfo, FileInfoHashKey> GetExtendedByHash();
    }
    public class FileInfoService : DataServiceBase<FileInfo, FileInfoId, FileInfoChangeset>, IFileInfoService
    {
        private IObservableCache<FileInfo, FileInfoHashKey> _fileHashCache;
        public IObservable<FileInfo> Get(FileInfoHashKey key)
            => _fileHashCache.WatchValue(key);


        private IDirectorySetupService _directorySetupService;
        private IUiDispatcherService _uiDispatcherService;
        public FileInfoService(IDirectorySetupService directorySetupService,
            IUiDispatcherService uiDispatcherService)
        {
            this._directorySetupService = directorySetupService;
            this._uiDispatcherService = uiDispatcherService;

            _getFullFileInfo = this._directorySetupService
                .Get()
                .Connect()
                .LeftJoinMany(
                this.cache.Connect(),
                (FileInfo dirSetup) => dirSetup.DirectorySetupId,
                    (left, right) => right.Items.Select(item => new ExtendedFileInfo(left, item))
                )
                .TransformMany(x => x, x => x.FileInfo.Id)
                .TakeUntil(destroy)
                .AsObservableCache();

            _getFullFIleInfoByHash = this._getFullFileInfo
                .Connect()
                .Filter(file => FileInfoHashKey.CanBuild(file.FileInfo))
                .ChangeKey(file => new FileInfoHashKey(file.FileInfo))
                .AsObservableCache();

            _fileHashCache = this.cache
                .Connect()
                .Filter(file => file.FileHash.HasValue && file.IsAccessible)
                .ChangeKey(file => new FileInfoHashKey(file))
                .AsObservableCache();
        }

        private readonly IObservableCache<ExtendedFileInfo, FileInfoId> _getFullFileInfo;
        public IObservableCache<ExtendedFileInfo, FileInfoId> GetExtended() => _getFullFileInfo;
        private readonly IObservableCache<ExtendedFileInfo, FileInfoHashKey> _getFullFIleInfoByHash;
        public IObservableCache<ExtendedFileInfo, FileInfoHashKey> GetExtendedByHash() => _getFullFIleInfoByHash;
        public IObservableCache<FileInfo, FileInfoId> Get(DirectorySetupId directorySetupId)
        {
            return this.cache.Connect()
                .Filter(fi => fi.DirectorySetupId == directorySetupId)
                .AsObservableCache();
        }


        IAsyncJobState SyncJobState;

        public void Synchronize()
        {

            try
            {
                if (SyncJobState != null && SyncJobState.State != AsyncState.Done)
                    return;
                var jobState = new AsyncJobState();
                this.SyncJobState = jobState;
                var setupProcessState = new AsyncProcessState();
                setupProcessState.TextChanges.OnNext("initial Setup");

                int taskCount = 8;
                int curTask = 0;
                setupProcessState.ProgressChanges.OnNext(new Progress(taskCount, curTask++));

                IEnumerable<DirectorySetup> dirSetups =
                    this._directorySetupService
                    .Get()
                    .KeyValues
                    .Select(x => x.Value).ToArray();

                setupProcessState.ProgressChanges.OnNext(new Progress(taskCount, curTask++));

                IEnumerable<ExtendedFileInfo> fileInfos =
                    this.GetExtended()
                    .KeyValues
                    .Select(x => x.Value).ToArray();

                setupProcessState.ProgressChanges.OnNext(new Progress(taskCount, curTask++));

                var actualDirSetupFiles =
                    dirSetups
                    .Where(dirSetup => dirSetup.IsValid)
                    .Select(dirSetup =>
                    new
                    {
                        files = Directory.GetFiles(dirSetup.Path.LocalPath, "*", SearchOption.AllDirectories),
                        dirSetup
                    })
                    .ToArray();

                setupProcessState.ProgressChanges.OnNext(new Progress(taskCount, curTask++));

                var flatDirSetupFiles = actualDirSetupFiles
                    .SelectMany(files => files.files.Select(path => new { files.dirSetup, path }));

                setupProcessState.ProgressChanges.OnNext(new Progress(taskCount, curTask++));

                IEnumerable<string> actualFiles = actualDirSetupFiles
                    .SelectMany(dirSetupFiles => dirSetupFiles.files);

                setupProcessState.ProgressChanges.OnNext(new Progress(taskCount, curTask++));

                var allPaths = fileInfos
                        .Select(x => x.AbsolutePath)
                        .Union(actualFiles)
                        .Distinct()
                        .Select(path => new { path, info = new SysFileInfo(path) });

                setupProcessState.ProgressChanges.OnNext(new Progress(taskCount, curTask++));

                IEnumerable<FileInfoChangeset> mergedFiles =
                    from file in allPaths
                    join foundFile in flatDirSetupFiles
                        on file.path equals foundFile.path into foundFiles
                    join definedFile in fileInfos
                        on file.path equals definedFile.AbsolutePath into definedFiles

                    select new FileInfoChangeset(definedFiles.Any() ? definedFiles.First().FileInfo as FileInfo? : null)
                    {
                        Path =
                            new Uri(Uri.UnescapeDataString(
                                (
                                definedFiles.Any()
                                    ? definedFiles.First().DirectorySetup
                                    : foundFiles.First().dirSetup
                                ).Path.MakeRelativeUri(new Uri(file.path))
                                .ToString()
                            ), UriKind.Relative),
                        CreationTime = file.info.CreationTime,
                        LastWriteTime = file.info.LastWriteTime,
                        FileSize = file.info.Length,
                        IsAccessible = foundFiles.Any(),
                        DirectorySetupId = foundFiles.Any() ? foundFiles.First().dirSetup.Id : definedFiles.First().DirectorySetup.Id
                    };

                setupProcessState.ProgressChanges.OnNext(new Progress(taskCount, curTask++));
                this.Patch(mergedFiles);
                jobState.StateChanges.OnNext(AsyncState.Done);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void UpdateHashes() => this.UpdateHashes(16);
        public void UpdateHashes(int threadcount)
        {
            CompositeDisposable finalDisposer = new CompositeDisposable();

            var changedFiles = this.GetExtended()
                .KeyValues
                .Select(file => new { file = file.Value, sysFileInfo = new SysFileInfo(file.Value.AbsolutePath) })
                .Where(file =>
                    file.sysFileInfo.Exists &&
                    (file.sysFileInfo.LastWriteTime != file.file.LastWriteTime || !file.file.FileHash.HasValue));

            uint i = 0;
            var groups = changedFiles
                .GroupBy(_ => i % threadcount);

            HashAlgorithm hashAlgorithm = SHA512.Create();
            hashAlgorithm.DisposeWith(finalDisposer);

            var tasks = groups
                .Select(group =>
                {
                    var result = new Subject<IEnumerable<FileInfoChangeset>>();
                    var log = new ReplaySubject<string>();
                    var files = group;
                    return new
                    {
                        files = files.ToArray(),
                        log,
                        result,
                        task = new Task(() =>
                        {
                            log.OnNext($"[{DateTime.Now}] starting...");

                            var res = files.Select(file =>
                             {
                                 {
                                     log.OnNext($"[{DateTime.Now}] hashing file '{file.file.AbsolutePath}'");
                                     try
                                     {
                                         return new FileInfoChangeset(file.file.FileInfo)
                                         {
                                             FileHash = _hashFile(hashAlgorithm, file.file.AbsolutePath),
                                             IsAccessible = File.Exists(file.file.AbsolutePath)
                                         };
                                     }
                                     catch (Exception ex)
                                     {
                                         log.OnNext($"[{DateTime.Now}] failed: {ex}");
                                     }
                                     return default;
                                 }
                             }).ToArray();

                            log.OnNext($"[{DateTime.Now}] done.");
                            result.OnNext(res);
                            log.OnCompleted();
                            result.OnCompleted();
                        })
                    };
                }).ToArray();

            int resCounter = 0;

            tasks.ToList().ForEach(taskWatcher =>
            {
                taskWatcher.result
                .Subscribe(result =>
                {
                    try
                    {
                        this._uiDispatcherService.UiDispatcher.Invoke(() =>
                        {
                            this.Patch(result);
                        });
                    }
                    catch (Exception ex)
                    {
                        taskWatcher.log.OnNext($"[{DateTime.Now}] could not path data: {ex}");
                    }

                    resCounter++;
                    if (resCounter != threadcount)
                        return;
                    finalDisposer.Dispose();
                });

            });


            tasks.ToList().ForEach(task => task.task.Start());
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
    }
}
