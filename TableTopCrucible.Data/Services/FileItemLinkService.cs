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

    }
    public class FileItemLinkService : DataServiceBase<FileItemLink, FileItemLinkId, FileItemLinkChangeset>, IFileItemLinkService
    {
        private IObservableCache<VersionedFile, FileInfoHashKey> _versionedFilesByHash { get; }
        public IObservableCache<VersionedFile, FileInfoHashKey> GetVersionedFilesByHash() => _versionedFilesByHash;
        private IObservableCache<FileItemLinkEx, FileItemLinkId> _getEx;
        public IObservableCache<FileItemLinkEx, FileItemLinkId> GetEx() => _getEx;

        public FileItemLinkService(
            IFileDataService fileDataService,
            ISettingsService settingsService,
            INotificationCenterService notificationCenter) : base(settingsService, notificationCenter)
        {

            _getEx = fileDataService
                .GetExtendedByHash()
                .Connect()
                .RightJoin(
                    this.Get()
                        .Connect(),
                    link => link.ThumbnailKey.HasValue ? link.ThumbnailKey.Value : default,
                    (file,  link) => new FileItemLinkEx(link, file.HasValue ? file.Value : null as FileInfoEx?))
                .ChangeKey(x=>x.Id)
                .AsObservableCache();


            _versionedFilesByHash = 
                GetEx()
               .Connect()
               .ChangeKey(link => link.FileKey)
               .LeftJoinMany(
                   fileDataService
                       .GetExtended()
                       .Connect()
                       .Filter(file => file.HashKey.HasValue),
                   (FileInfoEx file) => file.HashKey.Value,
                   (link, files) => new VersionedFile(link, files.Items)
               ).AsObservableCache();
        }

    }
}
