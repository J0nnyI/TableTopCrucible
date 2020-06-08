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
        public IObservableCache<ExtendedFileInfo, FileInfoId> GetFullFIleInfo();
        public IObservableCache<FileInfo, FileInfoId> Get(DirectorySetupId directorySetupId);
    }
    public class FileInfoService : DataServiceBase<FileInfo, FileInfoId, FileInfoChangeset>, IFileInfoService
    {


        private bool synchronizing = false;
        private IDirectorySetupService _directorySetupService;
        private IUiDispatcherService _uiDispatcherService;
        public FileInfoService(IDirectorySetupService directorySetupService,
            IUiDispatcherService uiDispatcherService)
        {
            this._directorySetupService = directorySetupService;
            this._uiDispatcherService = uiDispatcherService;

            _getFullFIleInfo = this._directorySetupService
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
        }

        private readonly IObservableCache<ExtendedFileInfo, FileInfoId> _getFullFIleInfo;
        public IObservableCache<ExtendedFileInfo, FileInfoId> GetFullFIleInfo() => _getFullFIleInfo;

        public IObservableCache<FileInfo, FileInfoId> Get(DirectorySetupId directorySetupId)
        {
            return this.cache.Connect()
                .Filter(fi => fi.DirectorySetupId == directorySetupId)
                .AsObservableCache();
        }

        public void Synchronize()
        {
            if (synchronizing)
                return;
            synchronizing = true;

            IEnumerable<DirectorySetup> dirSetups = this._directorySetupService
                .Get()
                .KeyValues
                .Select(x => x.Value);

            IEnumerable<ExtendedFileInfo> fileInfos = this.GetFullFIleInfo().KeyValues.Select(x => x.Value);

            var actualDirSetupFiles = dirSetups
                .Where(dirSetup => dirSetup.IsValid)
                .Select(dirSetup => new { files = Directory.GetFiles(dirSetup.Path.LocalPath, "*", SearchOption.AllDirectories), dirSetup }).ToArray();

            var flatDirSetupFiles = actualDirSetupFiles
                .SelectMany(files => files.files.Select(path => new { files.dirSetup, path }));

            IEnumerable<string> actualFiles = actualDirSetupFiles
                .SelectMany(dirSetupFiles => dirSetupFiles.files);

            var allPaths = fileInfos
                    .Select(x => x.AbsolutePath)
                    .Union(actualFiles)
                    .Distinct()
                    .Select(path => new { path, info = new SysFileInfo(path) });

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
                    IsAccessible = !foundFiles.Any(),
                    DirectorySetupId = foundFiles.Any() ? foundFiles.First().dirSetup.Id : definedFiles.First().DirectorySetup.Id
                };

            this.Patch(mergedFiles);
            synchronizing = false;
        }

        public void Hash()
        {

        }

        public void UpdateHashes() => this.UpdateHashes(16);
        public void UpdateHashes(int threadcount)
        {
            CompositeDisposable finalDisposer = new CompositeDisposable();

            var changedFiles = this.GetFullFIleInfo()
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

                            var res =files.Select(file =>
                            {
                                {
                                    log.OnNext($"[{DateTime.Now}] hashing file '{file.file.AbsolutePath}'");
                                    try
                                    {
                                        return new FileInfoChangeset(file.file.FileInfo)
                                        {
                                            FileHash = _hashFile(hashAlgorithm, file.file.AbsolutePath)
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
