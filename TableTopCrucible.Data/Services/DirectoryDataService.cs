using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.Services
{
    public interface IDirectoryDataService : IDataService<DirectorySetup, DirectorySetupId, DirectorySetupChangeset> { }
    public class DirectoryDataService : DataServiceBase<DirectorySetup, DirectorySetupId, DirectorySetupChangeset>, IDirectoryDataService
    {



    }
}
