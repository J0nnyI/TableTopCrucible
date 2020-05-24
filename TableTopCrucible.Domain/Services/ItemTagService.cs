using DynamicData;
using DynamicData.Alias;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Services
{
    public interface IItemTagService
    {
        IObservable<IEnumerable<Tag>> Get(ItemId item);
        IObservable<IEnumerable<Tag>> Get();
    }
    public class ItemTagService : IItemTagService
    {
        private readonly IItemService _itemService;
        public ItemTagService(IItemService itemService)
        {
            this._itemService = itemService;
        }

        public IObservable<IEnumerable<Tag>> Get(ItemId item)
            => _itemService.Get(item).Select(item => item.Tags);
        public IObservable<IEnumerable<Tag>> Get()
            => this._itemService
                .Get()
                .Connect()
                .RemoveKey()
                .QueryWhenChanged(items =>
                    items.SelectMany(x => x.Tags)
                .Distinct());



    }
}