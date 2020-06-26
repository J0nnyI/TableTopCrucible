using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Services;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.WPF.ViewModels
{
    public class MainViewModel : DisposableReactiveObjectBase
    {

        private readonly IItemService _itemService;
        public ItemListViewModel ItemList { get; }
        public CreateItemCommand CreateItem { get; }
        public ItemEditorViewModel ItemEditor { get; }
        public FileDefinitionViewModel FileDefinitions { get; }
        public DirectorySetupViewModel DirectorySetup { get; }
        public NotificationCenterViewModel NotificationCenter { get; }

        private ObservableAsPropertyHelper<Item?> _selectedItem;
        public Item? SelectedItem
        {
            get => _selectedItem.Value;
            set => this.ItemList.SelectedItem = value;
        }



        public MainViewModel(
            // services
            IItemService itemService,
            // commands
            CreateItemCommand createItem,
            // viewsModels
            ItemListViewModel itemList,
            ItemEditorViewModel itemEditor,
            FileDefinitionViewModel fileDefinitions,
            DirectorySetupViewModel directorySetup,
            NotificationCenterViewModel notificationCenter
            )
        {
            // services
            this._itemService = itemService ?? throw new NullReferenceException("got no itemservice");
            // commands
            this.CreateItem = createItem ?? throw new NullReferenceException("got no create item commend");
            // views
            this.ItemList = itemList ?? throw new NullReferenceException("got no itemlist");
            this.ItemEditor = itemEditor ?? throw new NullReferenceException("got no itemEditor");
            this.FileDefinitions = fileDefinitions ?? throw new NullReferenceException("got no file definitions editor");
            this.DirectorySetup = directorySetup ?? throw new NullReferenceException("got no dir setup editor");
            this.NotificationCenter = notificationCenter ?? throw new NullReferenceException("got no notification center");

            this._selectedItem = ItemList.SelectedItemChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(SelectedItem));


            this.CreateItem.ItemCreated += this._createItem_ItemCreated;


            _addItem("test 1");
            this._itemService.Patch(_getTaggyItem("taggy item 1"));
            this._itemService.Patch(_getTaggyItem("taggy item 2"));
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
                Name = name,
                Tags = new List<Tag> { (Tag)"Tag 1", (Tag)"Tag 2" },
                Thumbnail = "https://i.etsystatic.com/19002916/r/il/617e49/1885302518/il_fullxfull.1885302518_9ovo.jpg"
                //Thumbnail = (Thumbnail)@"D:\__MANAGED_FILES__\DnD\__Thumbnails__\20200126_191331.jpg"
            };
        }
        private ItemChangeset _getTaggyItem(string name)
        {
            return new ItemChangeset()
            {
                Name = name,
                Tags = new List<Tag> { (Tag)"Tag 0", (Tag)"Tag 1", (Tag)"Tag 2", (Tag)"Tag 3", (Tag)"Tag 4",
                                       (Tag)"Tag 5", (Tag)"Tag 6", (Tag)"Tag 7", (Tag)"Tag 8", (Tag)"Tag 9",
                                       (Tag)"Tag 10", (Tag)"Tag 11", (Tag)"Tag 12", (Tag)"Tag 13", (Tag)"Tag 14",
                                       (Tag)"Tag 15", (Tag)"Tag 16", (Tag)"Tag 17", (Tag)"Tag 18", (Tag)"Tag 19", },
                Thumbnail = "https://i.etsystatic.com/19002916/r/il/617e49/1885302518/il_fullxfull.1885302518_9ovo.jpg"
                //Thumbnail = (Thumbnail)@"D:\__MANAGED_FILES__\DnD\__Thumbnails__\20200126_191331.jpg"
            };
        }
    }
}
