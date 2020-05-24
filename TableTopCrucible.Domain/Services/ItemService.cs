using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Services
{
    public interface IItemService : IDataService<Item, ItemId, IItemChangeset> { }
    public class ItemService : DataServiceBase<Item, ItemId, IItemChangeset>, IItemService
    {
        public ItemService()
        {
        }

    }
}
