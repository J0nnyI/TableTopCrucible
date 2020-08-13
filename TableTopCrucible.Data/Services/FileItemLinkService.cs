using DynamicData;
using DynamicData.Kernel;

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;

using TableTopCrucible.Core.Services;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.Services
{
    public interface IFileItemLinkService : IDataService<FileItemLink, FileItemLinkId, FileItemLinkChangeset>
    {
        IObservableCache<VersionedFile, FileInfoHashKey> GetVersionedFilesByHash();
        IObservableCache<FileItemLinkEx, FileItemLinkId> GetEx();
        IObservable<IChangeSet<VersionedFile, FileInfoHashKey>> BuildversionedFiles(IObservable<IChangeSet<FileItemLink, FileItemLinkId>> cache);
        IObservable<IChangeSet<VersionedFile, FileInfoHashKey>> BuildversionedFiles(IObservable<IChangeSet<FileItemLinkEx, FileItemLinkId>> cache);
        IObservable<IChangeSet<FileItemLinkEx, FileItemLinkId>> BuildEx(IObservable<IChangeSet<FileItemLink, FileItemLinkId>> cache);
    }
    public class FileItemLinkService : DataServiceBase<FileItemLink, FileItemLinkId, FileItemLinkChangeset>, IFileItemLinkService
    {
        private IObservableCache<VersionedFile, FileInfoHashKey> _versionedFilesByHash { get; }
        public IObservableCache<VersionedFile, FileInfoHashKey> GetVersionedFilesByHash() => _versionedFilesByHash;
        private IObservableCache<FileItemLinkEx, FileItemLinkId> _getEx;
        private readonly IFileDataService fileDataService;

        public IObservableCache<FileItemLinkEx, FileItemLinkId> GetEx() => _getEx;

        public IObservable<IChangeSet<FileItemLinkEx, FileItemLinkId>> BuildEx(IObservable<IChangeSet<FileItemLink, FileItemLinkId>> cache)
        {
            return fileDataService
                .GetExtendedByHash()
                .Connect()
                .RightJoin(
                    cache,
                    link => link.ThumbnailKey.HasValue ? link.ThumbnailKey.Value : default,
                    (file, link) => new FileItemLinkEx(link, file.HasValue ? file.Value : null as FileInfoEx?))
                .ChangeKey(x => x.Id);
        }

        public IObservable<IChangeSet<VersionedFile, FileInfoHashKey>> BuildversionedFiles(IObservable<IChangeSet<FileItemLink, FileItemLinkId>> cache)
            => BuildversionedFiles(this.BuildEx(cache));
        public IObservable<IChangeSet<VersionedFile, FileInfoHashKey>> BuildversionedFiles(IObservable<IChangeSet<FileItemLinkEx, FileItemLinkId>> cache)
        {
            return cache
           .ChangeKey(link => link.FileKey)
           .LeftJoinMany(
               fileDataService
                   .GetExtended()
                   .Connect()
                   .Filter(file => file.HashKey.HasValue),
               (FileInfoEx file) => file.HashKey.Value,
               (link, files) => new VersionedFile(link, files.Items));
        }

        public FileItemLinkService(
            IFileDataService fileDataService,
            ISettingsService settingsService,
            INotificationCenterService notificationCenter) : base(settingsService, notificationCenter)
        {
            this.fileDataService = fileDataService;

            _getEx = BuildEx(this.cache.Connect()).AsObservableCache();

            _versionedFilesByHash = BuildversionedFiles(_getEx.Connect()).AsObservableCache();
        }

    }
}