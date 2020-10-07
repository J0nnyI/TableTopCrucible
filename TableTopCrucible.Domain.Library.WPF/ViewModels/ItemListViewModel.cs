
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
using System.Windows.Input;
using System.Windows.Controls;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Reactive.Disposables;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public class ItemSelectionInfo : ReactiveObject
    {
        [Reactive]
        public bool IsSelected { get; set; } = false;
        public ItemId ItemId { get; }
        public ItemSelectionInfo(ItemId itemId)
        {
            this.ItemId = itemId;
        }
    }

    public class SelectedItemView
    {
        public SelectedItemView(ItemEx Item, ItemSelectionInfo selectionInfo)
        {
            this.Item = Item;
            SelectionInfo = selectionInfo;
            if (selectionInfo != null && Item.ItemId != selectionInfo?.ItemId)
                throw new InvalidOperationException("the item-ids do not match");
        }

        public ItemEx Item { get; }
        public ItemSelectionInfo SelectionInfo { get; }
    }


    public class ItemListViewModel : DisposableReactiveObjectBase
    {
        private readonly IItemService _itemService;
        private readonly IInjectionProviderService _injectionProviderService;

        public SourceCache<SelectedItemView, ItemId> Selections { get; } = new SourceCache<SelectedItemView, ItemId>(item => item.SelectionInfo.ItemId);

        public BehaviorSubject<Func<ItemEx, bool>> FilterChanges { get; } = new BehaviorSubject<Func<ItemEx, bool>>(_ => true);
        ReadOnlyObservableCollection<SelectedItemView> _items;
        public ReadOnlyObservableCollection<SelectedItemView> Items => _items;
        public IObservableList<ItemEx> Selection { get; private set; }
        public ICommand SelectionChangesCommand { get; }
        private bool disconnected = false;

        public event EventHandler<IEnumerable<ItemEx>> OnSelectionRestore;

        public ItemListViewModel(
            IItemService itemService,
            IInjectionProviderService injectionProviderService)
        {
            this._itemService = itemService ?? throw new NullReferenceException("got no itemService");
            this._injectionProviderService = injectionProviderService ?? throw new NullReferenceException("got no itemservice");


            var selectionChanges = new Subject<Unit>();
            selectionChanges.DisposeWith(disposables);
            this.SelectionChangesCommand = new RelayCommand(_ => selectionChanges.OnNext(new Unit()));


            this._injectionProviderService.Provider.Subscribe(
                (provider) =>
            {
                if (provider == null)
                    throw new InvalidOperationException("provider is null");

                var itemList = this._itemService
                    .GetExtended()
                    .Connect()
                    .DisposeMany()
                    .Filter(FilterChanges)
                    .TakeUntil(destroy)
                    .Sort(item => item.Name);

                var selectionList = itemList.RemoveKey()
                    .DistinctValues(item => item.ItemId)
                    .Transform(id => new ItemSelectionInfo(id), false)
                    .AddKey(selItem => selItem.ItemId);
                selectionList.Subscribe(_ =>
                {

                });
                var selection = itemList
                    .LeftJoin(
                        selectionList,
                        sel => sel.ItemId,
                        (item, selection) => new SelectedItemView(item, selection.HasValue ? selection.Value : null))
                    .AsObservableCache();
                selection.Connect().Subscribe(_ =>
                {

                });

                Selection = selection.Connect()
                    .Filter(selItem => selItem?.SelectionInfo?.IsSelected == true, selectionChanges.Where(_ => !this.disconnected))
                    .Transform(selItem => selItem.Item)
                    .RemoveKey()
                    .AsObservableList();

                IEnumerable<ItemId> selectionRestore = null;

                selection.Connect()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Do(_ =>
                    {
                        disconnected = true;
                        selectionRestore = Selection.Items.Select(item => item.ItemId);
                    })
                    .Bind(out _items)
                    .Do(_ =>
                    {
                        var x = selection.KeyValues.WhereIn(selectionRestore, item => item.Key)
                            .ToList();
                        selection.KeyValues.WhereIn(selectionRestore, item => item.Key)
                            .ToList()
                            .ForEach(item =>
                            {
                                item.Value.SelectionInfo.IsSelected = true;
                            });
                        disconnected = false;
                    })
                    .Subscribe();

            });

        }




    }
}
