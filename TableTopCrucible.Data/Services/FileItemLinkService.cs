using DynamicData;
using DynamicData.Kernel;

using System;
using System.Collections.Generic;
using System.Linq;
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
        IObservableCache<IGroup<VersionedFile, FileItemLinkId, FileInfoHashKey>, FileInfoHashKey> GetVersionedFilesByHash();
        IObservableCache<VersionedFile, FileItemLinkId> GetVersionedFiles();
        IObservableCache<FileItemLinkEx, FileItemLinkId> GetEx();
        IObservable<IChangeSet<VersionedFile, FileItemLinkId>> BuildversionedFiles(IObservable<IChangeSet<FileItemLink, FileItemLinkId>> cache);
        IObservable<IChangeSet<VersionedFile, FileItemLinkId>> BuildversionedFiles(IObservable<IChangeSet<FileItemLinkEx, FileItemLinkId>> cache);
        IObservable<IChangeSet<FileItemLinkEx, FileItemLinkId>> BuildEx(IObservable<IChangeSet<FileItemLink, FileItemLinkId>> cache);
    }
    public class FileItemLinkService : DataServiceBase<FileItemLink, FileItemLinkId, FileItemLinkChangeset>, IFileItemLinkService
    {
        private readonly IObservableCache<VersionedFile, FileItemLinkId> _versionedFiles;
        public IObservableCache<VersionedFile, FileItemLinkId> GetVersionedFiles() => _versionedFiles;
        private IObservableCache<IGroup<VersionedFile, FileItemLinkId, FileInfoHashKey>, FileInfoHashKey> _versionedFilesByHash { get; }
        public IObservableCache<IGroup<VersionedFile, FileItemLinkId, FileInfoHashKey>, FileInfoHashKey> GetVersionedFilesByHash() => _versionedFilesByHash;
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

        public IObservable<IChangeSet<VersionedFile, FileItemLinkId>> BuildversionedFiles(IObservable<IChangeSet<FileItemLink, FileItemLinkId>> cache)
            => BuildversionedFiles(this.BuildEx(cache));
        public IObservable<IChangeSet<VersionedFile, FileItemLinkId>> BuildversionedFiles(IObservable<IChangeSet<FileItemLinkEx, FileItemLinkId>> cache)
        {
            return fileDataService
                .GetExtended()
                .Connect()
                .Filter(file => file.HashKey.HasValue)
                .GroupWithImmutableState(file => file.HashKey)
                .ChangeKey(file => file.Key.Value)
                .LeftJoinMany(
                     GetEx().Connect(),
                    (FileItemLinkEx link) => link.FileKey,
                    (files, links) => new { files, links })
                .TransformMany(x =>x.links.Items.Select(link=> new VersionedFile(link,x.files.Items)), vFile=>vFile.Link.Id);
        }

        public FileItemLinkService(
            IFileDataService fileDataService,
            ISettingsService settingsService,
            INotificationCenterService notificationCenter) : base(settingsService, notificationCenter)
        {
            this.fileDataService = fileDataService;

            _getEx = BuildEx(this.cache.Connect()).AsObservableCache();
            _versionedFiles = BuildversionedFiles(_getEx.Connect()).AsObservableCache();
            _versionedFilesByHash = _versionedFiles.Connect().Group(x => x.HashKey).AsObservableCache();            
        }

    }
}