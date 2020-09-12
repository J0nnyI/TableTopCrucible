﻿
using DynamicData;

using ReactiveUI;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Core.Helper;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public class ItemListViewModel : DisposableReactiveObjectBase
    {
        private readonly IItemService _itemService;
        private readonly IInjectionProviderService _injectionProviderService;

        public BehaviorSubject<Func<ItemEx, bool>> FilterChanges { get; } = new BehaviorSubject<Func<ItemEx, bool>>(_ => true);
        ReadOnlyObservableCollection<ItemEx> _items;
        public ReadOnlyObservableCollection<ItemEx> Items => _items;

        #region reactive properties

        private BehaviorSubject<ItemEx?> _selectedItemChanges { get; } = new BehaviorSubject<ItemEx?>(null);
        public IObservable<ItemEx?> SelectedItemChanges => _selectedItemChanges;
        private readonly ObservableAsPropertyHelper<ItemEx?> _selectedItem;
        public ItemEx? SelectedItem
        {
            get => _selectedItem.Value;
            set
            {
                if (disconnected)
                    return;
                _selectedItemChanges.OnNext(value);
            }
        }
        ItemEx? _selectedItemBuffer = null;

        #endregion

        private bool disconnected = false;

        public ItemListViewModel(
            IItemService itemService,
            IInjectionProviderService injectionProviderService)
        {
            this._itemService = itemService ?? throw new NullReferenceException("got no itemService");
            this._injectionProviderService = injectionProviderService ?? throw new NullReferenceException("got no itemservice");

            this._injectionProviderService.Provider.Subscribe(
                (provider) =>
            {
                if (provider == null)
                    throw new InvalidOperationException("provider is null");

                this._itemService
                .GetExtended()
                .Connect()
                .DisposeMany()
                .Filter(FilterChanges)
                .TakeUntil(destroy)
                .Sort(item=>item.Name)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(_ =>
                {
                    this.disconnected = true;
                    _selectedItemBuffer = SelectedItem;
                })
                .Bind(out _items)
                .Do(_ =>
                {
                    this.disconnected = false;
                    this.SelectedItem = _items.FirstOrDefault(x => x.SourceItem.Id == _selectedItemBuffer?.SourceItem.Id);
                })
                .Subscribe();
            });


            this._selectedItem =
                SelectedItemChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(SelectedItem));

            this.disposables.Add(_selectedItemChanges, _selectedItem);
        }
    }
}
