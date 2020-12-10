using DynamicData;

using ReactiveUI;

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

using TableTopCrucible.Core.Models.Enums;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.ValueTypes;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using TableTopCrucible.WPF.Helper;

using FileInfo = TableTopCrucible.Data.Models.Sources.FileInfo;
using SysFileInfo = System.IO.FileInfo;
using TableTopCrucible.Data.Models.Sources;

namespace TableTopCrucible.Data.Services
{
    public interface IModelFileDataService : IFileDataService { }
    public interface IImageFileDataService : IFileDataService { }
    public interface IFileDataService : IDataService<FileInfo, FileInfoId, FileInfoChangeset>
    {
        public IObservableCache<FileInfoEx, FileInfoId> GetExtended();
        public IObservableCache<FileInfo, FileInfoId> Get(DirectorySetupId directorySetupId);
        public IObservableCache<FileInfoEx, FileInfoHashKey> GetExtendedByHash();
    }
    public class FileDataService : DataServiceBase<FileInfo, FileInfoId, FileInfoChangeset>, IFileDataService
    {
        private struct DirSetupFile
        {
            public DirSetupFile(string path, DirectorySetup dirSetup)
            {
                Path = path ?? throw new ArgumentNullException(nameof(path));
                DirSetup = dirSetup;
            }

            public string Path { get; }
            public DirectorySetup DirSetup { get; }
        }

        private IObservableCache<FileInfo, FileInfoHashKey> _fileHashCache;
        public IObservable<FileInfo> Get(FileInfoHashKey key)
            => _fileHashCache.WatchValue(key);


        private readonly IDirectoryDataService _directorySetupService;
        private readonly INotificationCenterService _notificationCenterService;
        private readonly ISettingsService _settingsService;

        public FileDataService(
            IDirectoryDataService directorySetupService,
            INotificationCenterService notificationCenterService,
            ISettingsService settingsService)
            : base(settingsService, notificationCenterService)
        {
            this._directorySetupService = directorySetupService;
            this._notificationCenterService = notificationCenterService;
            this._settingsService = settingsService;
            _getFullFileInfo = this._directorySetupService
                .Get()
                .Connect()
                .LeftJoinMany(
                this.cache.Connect(),
                (dirSetup) => dirSetup.DirectorySetupId,
                    (left, right) => right.Items.Select(item => new FileInfoEx(left, item))
                )
                .TransformMany(x => x, x => x.FileInfo.Id)
                .TakeUntil(destroy)
                .AsObservableCache();

            _getFullFIleInfoByHash = this._getFullFileInfo
                .Connect()
                .Filter(file => FileInfoHashKey.CanBuild(file.FileInfo))
                .ChangeKey(file => file.FileInfo.HashKey.Value)
                .AsObservableCache();

            _fileHashCache = this.cache
                .Connect()
                .Filter(file => file.FileHash.HasValue && file.IsAccessible)
                .ChangeKey(file => file.HashKey.Value)
                .AsObservableCache();
        }

        private readonly IObservableCache<FileInfoEx, FileInfoId> _getFullFileInfo;
        public IObservableCache<FileInfoEx, FileInfoId> GetExtended() => _getFullFileInfo;
        private readonly IObservableCache<FileInfoEx, FileInfoHashKey> _getFullFIleInfoByHash;
        public IObservableCache<FileInfoEx, FileInfoHashKey> GetExtendedByHash() => _getFullFIleInfoByHash;

        public IObservableCache<FileInfo, FileInfoId> Get(DirectorySetupId directorySetupId)
        {
            return this.cache.Connect()
                .Filter(fi => fi.DirectorySetupId == directorySetupId)
                .AsObservableCache();
        }


    }
}
