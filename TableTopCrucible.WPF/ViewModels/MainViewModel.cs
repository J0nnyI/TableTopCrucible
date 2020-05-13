using DynamicData;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Services;
using TableTopCrucible.Domain.ValueTypes;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.WPF.ViewModels
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private ReadOnlyObservableCollection<Item> _items;
        public ReadOnlyObservableCollection<Item> Items
        {
            get => _items;
            set => this.onPropertyChange(value, ref _items);
        }

        private Item _selectedItem;
        public Item SelectedItem
        {
            get => _selectedItem;
            set => this.onPropertyChange(value, ref _selectedItem);
        }

        private List<IDisposable> subs = new List<IDisposable>();
        IItemService itemService;
        public ItemEditorViewModel ItemEditor { get; } = new ItemEditorViewModel();
        public ItemListViewModel ItemList { get; } = new ItemListViewModel();
        public CreateItemCommand CreateItem { get; }



        public MainViewModel(IItemService itemService, CreateItemCommand createItem)
        {
            this.itemService = itemService;
            this.CreateItem = createItem;
            this.CreateItem.ItemCreated += this.CreateItem_ItemCreated;


            addItem("test 1");
            //addItem("test 2");
            subs.Add(itemService
                .Get()
                .Bind(out this._items)
                .Subscribe());
            //addItem("test 3");
            //addItems("test 2.1", "test 2.2", "test 2.3");

            

            this.raisePropertyChanged(nameof(this.Items));
        }

        private void CreateItem_ItemCreated(object sender, ItemCreatedEventArgs e)
        {
            this.SelectedItem = e.Item;
        }

        private void addItem(string name)
        {
            itemService.Patch(getItem(name));
        }
        private void addItems(params string[] names)
        {
            itemService.Patch(names.Select(name => getItem(name)));
        }
        private ItemChangeset getItem(string name)
        {
            return new ItemChangeset()
            {
                Name = (ItemName)name,
                Tags = new List<Tag> { (Tag)"Tag 1", (Tag)"Tag 2" },
                Thumbnail = (Thumbnail)"https://i.etsystatic.com/19002916/r/il/617e49/1885302518/il_fullxfull.1885302518_9ovo.jpg"
                //Thumbnail = (Thumbnail)@"D:\__MANAGED_FILES__\DnD\__Thumbnails__\20200126_191331.jpg"
            };
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    subs.ForEach(sub => sub.Dispose());
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
