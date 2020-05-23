
using DynamicData;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Data;
using TableTopCrucible.Domain.Models;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Services;
using TableTopCrucible.Domain.ValueTypes;
using TableTopCrucible.WPF.Commands;
using TableTopCrucible.WPF.Helper;

namespace TableTopCrucible.WPF.ViewModels
{
    public class ItemListViewModel : DisposableReactiveObject
    {
        private readonly IItemService _itemService;
        private readonly IInjectionProviderService _injectionProviderService;
        public CreateItemCommand CreateItemCommand { get; }

        ReadOnlyObservableCollection<ItemCardViewModel> _items;
        public ReadOnlyObservableCollection<ItemCardViewModel> Items => _items;
        [Reactive]
        public CollectionViewSource ItemsDataView { get; private set; }

        #region reactive properties

        private readonly BehaviorSubject<ItemCardViewModel> _selectedItemVmChanges = new BehaviorSubject<ItemCardViewModel>(null);
        private IObservable<ItemCardViewModel> SelectedItemVmChanges => _selectedItemVmChanges;
        private readonly ObservableAsPropertyHelper<ItemCardViewModel> _selectedItemVm;
        public ItemCardViewModel SelectedItemVm
        {
            get => _selectedItemVm.Value;
            set => _selectedItemVmChanges.OnNext(value);
        }
        public IObservable<Item?> SelectedItemChanges { get; }
        private readonly ObservableAsPropertyHelper<Item?> _selectedItem;
        public Item? SelectedItem
        {
            get => _selectedItem.Value;
            set => selectItem(value);
        }

        #endregion

        private void selectItem(Item? item)
        {
            if (!item.HasValue)
                this.SelectedItemVm = null;
            this.SelectedItemVm = Items.FirstOrDefault(curItem => curItem?.Item?.Id == item?.Id);
        }

        public ItemListViewModel(
            IItemService itemService,
            IInjectionProviderService injectionProviderService,
            CreateItemCommand CreateItemCommand)
        {
            this._itemService = itemService ?? throw new NullReferenceException("got no itemService");
            this._injectionProviderService = injectionProviderService ?? throw new NullReferenceException("got no itemservice");
            this.CreateItemCommand = CreateItemCommand;

            this._injectionProviderService.Provider.Subscribe(
                (provider) =>
            {
                if (provider == null)
                    return;
                this._itemService
                .Get()
                .Connect()
                .Transform((item) =>
                {
                    var viewModel = provider.GetRequiredService<ItemCardViewModel>();
                    viewModel.ItemChanges.OnNext(item);
                    return viewModel;
                })
                .DisposeMany()
                .Bind(out _items)
                .TakeUntil(destroy)
                .Subscribe(x=> { 
                    if(this.ItemsDataView == null)
                    {
                        this.ItemsDataView = new CollectionViewSource()
                        {
                            Source = this.Items,
                        };
                        this.Sort(nameof(ItemName));
                    }
                });

            });

            this.SelectedItemChanges =
                _selectedItemVmChanges
                .Select(vm => vm?.Item)
                .TakeUntil(destroy);

            this._selectedItem =
                SelectedItemChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(SelectedItem));
            this._selectedItemVm =
                SelectedItemVmChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(SelectedItemVm));
        }
        private void Sort(string sortBy, ListSortDirection direction = ListSortDirection.Ascending)
        {
            ItemsDataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            ItemsDataView.SortDescriptions.Add(sd);
            ItemsDataView.View.Refresh();
        }
    }
}
