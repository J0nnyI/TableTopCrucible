using DynamicData;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Threading.Tasks;

using TableTopCrucible.Core.Enums;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Core.Utilities;
using TableTopCrucible.Core.ValueTypes;
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
        private INotificationCenterService _notificationCenterService;

        public FileInfoService(
            IDirectorySetupService directorySetupService,
            IUiDispatcherService uiDispatcherService,
            INotificationCenterService notificationCenterService)
        {
            this._directorySetupService = directorySetupService;
            this._uiDispatcherService = uiDispatcherService;
            this._notificationCenterService = notificationCenterService;

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


        private bool synchronizing = false;

        public void Synchronize()
        {

            AsyncProcessState setupProcessState;
            AsyncJobState setupJobState = this._notificationCenterService.CreateSingleTaskJob(out setupProcessState, "Synchronizing Files");

            setupJobState.ProcessChanges.OnNext(setupProcessState.AsArray());

            try
            {
                if (synchronizing)
                    return;
                synchronizing = true;
                setupProcessState.TitleChanges.OnNext("initial Setup");

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
                setupProcessState.StateChanges.OnNext(AsyncState.Done);
            }
            catch (Exception ex)
            {
                setupProcessState.StateChanges.OnNext(AsyncState.Failed);
                setupProcessState.ErrorChanges.OnNext(ex.ToString());
            }
            finally
            {
                setupProcessState.Complete();
                synchronizing = false;
            }
        }

        public void UpdateHashes() => this.UpdateHashes(16);
        public void UpdateHashes(int threadcount)
        {

            var job = new AsyncJobState("hashing the files");
            var prepProcess = new AsyncProcessState("preparing");
            var hashing = Enumerable.Range(1, threadcount).Select(x => new AsyncProcessState($"hashing #{x}", "", this._uiDispatcherService.UiDispatcher)).ToArray();
            var finalizer = new AsyncProcessState("finalizing", "", this._uiDispatcherService.UiDispatcher);

            finalizer.AddProgress(threadcount);

            var processes = new List<AsyncProcessState>();
            processes.Insert(0, finalizer);
            processes.InsertRange(0, hashing.ToList());
            processes.Insert(0, prepProcess);
            job.ProcessChanges.OnNext(processes);
            this._notificationCenterService.Register(job);

            prepProcess.State = AsyncState.InProgress;

            try
            {
                prepProcess.AddProgress(1, "reading files");



                CompositeDisposable finalDisposer = new CompositeDisposable();

                var changedFiles = this.GetExtended()
                    .KeyValues
                    .Select(file => new { file = file.Value, sysFileInfo = new SysFileInfo(file.Value.AbsolutePath) })
                    .Where(file =>
                        file.sysFileInfo.Exists &&
                        (file.sysFileInfo.LastWriteTime != file.file.LastWriteTime || !file.file.FileHash.HasValue))
                    .ToArray();

                prepProcess.OnNextStep("grouping files");

                uint i = 0;
                var groups = changedFiles
                    .GroupBy(_ => Convert.ToInt32(decimal.Remainder(i++, threadcount)))
                    .ToArray();

                prepProcess.OnNextStep("creating tasks");

                HashAlgorithm hashAlgorithm = SHA512.Create();
                hashAlgorithm.DisposeWith(finalDisposer);


                var tasks = groups
                    .Select(group =>
                    {
                        var result = new Subject<IEnumerable<FileInfoChangeset>>();
                        var files = group;
                        return new
                        {
                            files = files.ToArray(),
                            result,
                            task = new Task(() =>
                            {
                                var proc = hashing[group.Key];
                                try
                                {
                                    proc.AddProgress(files.Count(), "hashing...");

                                    var res = files.Select(file =>
                                    {
                                        {
                                            proc.OnNextStep();
                                            proc.Details = $"[{DateTime.Now}] hashing file '{file.file.AbsolutePath}'";
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
                                                proc.State = AsyncState.Failed;
                                                proc.Errors += ex.ToString();
                                            }
                                            return default;
                                        }
                                    }).ToArray();

                                    result.OnNext(res);
                                    result.OnCompleted();
                                }
                                catch (Exception ex)
                                {
                                    proc.State = AsyncState.Failed;
                                    proc.Errors += ex.ToString();
                                }
                            })
                        };
                    }).ToArray();

                int resCounter = 0;

                prepProcess.OnNextStep("creating finalizer");

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
                                finalizer.OnNextStep($"done with thread #{resCounter}");
                            });
                        }
                        catch (Exception ex)
                        {
                            finalizer.Errors += $"error on task #{resCounter}:{Environment.NewLine}{ex}{Environment.NewLine}";
                        }

                        resCounter++;
                        if (resCounter != threadcount)
                            return;
                        finalDisposer.Dispose();
                    });

                });

                prepProcess.OnNextStep("launching tasks");

                tasks.ToList().ForEach(task => task.task.Start());
                prepProcess.OnNextStep("done");
                prepProcess.State = AsyncState.Done;
            }
            catch (Exception ex)
            {
                prepProcess.Details += "Task failes" + Environment.NewLine;
                prepProcess.Errors = ex.ToString();
                prepProcess.State = AsyncState.Failed;
            }
            finally
            {
                prepProcess.Complete();
            }

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
