using DynamicData;
using DynamicData.Alias;

using System.Linq;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using TableTopCrucible.Domain.Models.Views;

namespace TableTopCrucible.Domain.Services
{
    public interface IFileItemLinkDataService : IDataService<FileItemLink, FileItemLinkId, FileItemLinkChangeset>
    {

    }
    public class FileItemLinkDataService : DataServiceBase<FileItemLink, FileItemLinkId, FileItemLinkChangeset>, IFileItemLinkDataService
    {
        private readonly IObservableCache<FileItemLink, FileInfoHashKey> _fileViewCache;
        private readonly IObservableCache<LinkedFile, FileItemLinkId> _linkedFile;

        public FileItemLinkDataService(IFileInfoService fileService)
        {
            this._fileViewCache = this.cache.Connect()
                .RemoveKey()
                .AddKey(x => x.FileKey)
                .AsObservableCache();


            this._linkedFile = this._fileViewCache.Connect()
                .LeftJoinMany(
                    fileService.GetExtended().Connect(),
                    (ExtendedFileInfo file) => new FileInfoHashKey(file.FileInfo),
                    (FileItemLink link, IGrouping<ExtendedFileInfo, FileInfoId, FileInfoHashKey> files)
                        => files.Items.Select(file => new LinkedFile(file, link))
                    )
                .TransformMany(files => files, (LinkedFile file) => file.Link.Id)
                .AsObservableCache();
        }
    }
}
