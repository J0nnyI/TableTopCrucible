
using DynamicData;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Data;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Core.Utilities;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public class ItemListViewModel : DisposableReactiveObjectBase
    {
        private readonly IItemService _itemService;
        private readonly IInjectionProviderService _injectionProviderService;
        public CreateItemCommand CreateItemCommand { get; }

        ReadOnlyObservableCollection<ExtendedItem> _items;
        public ReadOnlyObservableCollection<ExtendedItem> Items => _items;
        [Reactive]
        public CollectionViewSource ItemsDataView { get; private set; }

        #region reactive properties

        private BehaviorSubject<ExtendedItem?> _selectedItemChanges { get; } = new BehaviorSubject<ExtendedItem?>(null);
        public IObservable<ExtendedItem?> SelectedItemChanges => _selectedItemChanges;
        private readonly ObservableAsPropertyHelper<ExtendedItem?> _selectedItem;
        public ExtendedItem? SelectedItem
        {
            get => _selectedItem.Value;
            set => _selectedItemChanges.OnNext(value);
        }

        #endregion


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
                    throw new InvalidOperationException("provider is null");


                this._itemService
                .GetExtended()
                .Connect()
                .DisposeMany()
                .TakeUntil(destroy)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _items)
                .Subscribe(x =>
                {
                    if (this.ItemsDataView == null)
                    {
                        this.ItemsDataView = new CollectionViewSource()
                        {
                            Source = this.Items,
                        };
                        this.Sort(nameof(ItemName));
                    }
                },
                (Exception ex)=>
                {
                    Debugger.Break();
                });

            });


            this._selectedItem =
                SelectedItemChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(SelectedItem));

            this.disposables.Add(_selectedItemChanges, _selectedItem);
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
