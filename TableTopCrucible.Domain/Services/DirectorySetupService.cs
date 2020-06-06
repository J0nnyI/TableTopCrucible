
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Services
{
    public interface IDirectorySetupService : IDataService<DirectorySetup, DirectorySetupId, DirectorySetupChangeset> { }
    public class DirectorySetupService : DataServiceBase<DirectorySetup, DirectorySetupId, DirectorySetupChangeset>, IDirectorySetupService
    {



    }
}
