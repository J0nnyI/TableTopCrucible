using DynamicData;
using DynamicData.Alias;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using TableTopCrucible.Domain.ValueTypes;
using TableTopCrucible.Domain.ValueTypes.IDs;

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