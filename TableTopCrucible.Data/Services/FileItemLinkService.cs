using DynamicData;

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
    }
    public class FileItemLinkService : DataServiceBase<FileItemLink, FileItemLinkId, FileItemLinkChangeset>, IFileItemLinkService
    {
        private IObservableCache<VersionedFile, FileInfoHashKey> _versionedFilesByHash { get; }
        public IObservableCache<VersionedFile, FileInfoHashKey> GetVersionedFilesByHash() => _versionedFilesByHash;

        public FileItemLinkService(
            IFileDataService fileDataService,
            ISettingsService settingsService,
            INotificationCenterService notificationCenter) : base(settingsService, notificationCenter)
        {
            _versionedFilesByHash = this.Get()
               .Connect()
               .ChangeKey(link => link.FileKey)
               .LeftJoinMany(
                   fileDataService
                       .GetExtended()
                       .Connect()
                       .Filter(file=>file.HashKey.HasValue),
                   (ExtendedFileInfo file) => file.HashKey.Value,
                   (link, files) =>new VersionedFile( link, files.Items)
               ).AsObservableCache();
        }

    }
}
