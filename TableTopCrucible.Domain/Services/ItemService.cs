using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Services
{
    public interface IItemService : IDataService<Item, ItemId, ItemChangeset> { }
    public class ItemService : DataServiceBase<Item, ItemId, ItemChangeset>, IItemService
    {
        public ItemService()
        {
        }

    }
}
