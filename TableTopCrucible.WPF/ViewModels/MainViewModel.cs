using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Services;
using TableTopCrucible.Domain.ValueTypes;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.WPF.ViewModels
{
    public class MainViewModel : DisposableReactiveObject
    {
        [Reactive]
        public Item SelectedItem { get; set; }

        private readonly IItemService _itemService;
        public ItemListViewModel ItemList { get; }
        public ItemListViewModel ItemList2 { get; }
        public CreateItemCommand CreateItem { get; }



        public MainViewModel(
            // services
            IItemService itemService,
            // commands
            CreateItemCommand createItem,
            // views
            ItemListViewModel itemList,
            ItemListViewModel itemList2
            )
        {
            // services
            this._itemService = itemService ?? throw new NullReferenceException("got no itemservice");
            // commands
            this.CreateItem = createItem ?? throw new NullReferenceException("got no create item commend");
            // views
            this.ItemList = itemList ?? throw new NullReferenceException("got no itemlist");
            this.ItemList2 = itemList2 ?? throw new NullReferenceException("got no itemlist");
            if (object.ReferenceEquals(itemList, itemList2))
                throw new InvalidDataException("the viewmodesl must not be the same");


            this.CreateItem.ItemCreated += this._createItem_ItemCreated;


            _addItem("test 1");
        }

        private void _createItem_ItemCreated(object sender, ItemCreatedEventArgs e) 
            => this.SelectedItem = e.Item;

        private void _addItem(string name) 
            => _itemService.Patch(_getItem(name));
        private void _addItems(params string[] names) 
            => _itemService.Patch(names.Select(name => _getItem(name)));
        private ItemChangeset _getItem(string name)
        {
            return new ItemChangeset()
            {
                Name = (ItemName)name,
                Tags = new List<Tag> { (Tag)"Tag 1", (Tag)"Tag 2" },
                Thumbnail = (Thumbnail)"https://i.etsystatic.com/19002916/r/il/617e49/1885302518/il_fullxfull.1885302518_9ovo.jpg"
                //Thumbnail = (Thumbnail)@"D:\__MANAGED_FILES__\DnD\__Thumbnails__\20200126_191331.jpg"
            };
        }
        private ItemChangeset _getTaggyItem(string name)
        {
            return new ItemChangeset()
            {
                Name = (ItemName)name,
                Tags = new List<Tag> { (Tag)"Tag 1", (Tag)"Tag 2", (Tag)"Tag 3", (Tag)"Tag 4" , (Tag)"Tag 5", (Tag)"Tag 6" },
                Thumbnail = (Thumbnail)"https://i.etsystatic.com/19002916/r/il/617e49/1885302518/il_fullxfull.1885302518_9ovo.jpg"
                //Thumbnail = (Thumbnail)@"D:\__MANAGED_FILES__\DnD\__Thumbnails__\20200126_191331.jpg"
            };
        }
    }
}
