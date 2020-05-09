using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Automation;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Services;
using TableTopCrucible.Domain.ValueTypes;
using TableTopCrucible.Domain.ValueTypes.IDs;

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

        private List<IDisposable> subs = new List<IDisposable>();
        IItemService itemService;
        public ItemEditorViewModel ItemEditor { get; } = new ItemEditorViewModel();
        public ItemListViewModel ItemList { get; } = new ItemListViewModel();
        public MainViewModel(IItemService itemService)
        {
            this.itemService = itemService;
            addItem("test 1");
            addItem("test 2");
            subs.Add(itemService
                .Get()
                .Bind(out this._items)
                .Subscribe());
            addItem("test 3");
            addItems("test 2.1", "test 2.2", "test 2.3");

            this.raisePropertyChanged(nameof(this.Items));
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
                Tags = new List<Tag> { (Tag)"Tag 1", (Tag)"Tag 2" }
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
