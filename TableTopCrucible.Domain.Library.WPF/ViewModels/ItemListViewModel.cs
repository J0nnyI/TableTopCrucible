
using DynamicData;

using ReactiveUI;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Collections.Generic;
using System.Windows;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Core.Helper;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public interface ISelectionProvider
    {
        ISourceList<ItemId> SelectedItemIDs { get; }
        bool Disconnected { get; }
    }
    public class ItemSelectionInfo : DisposableReactiveObjectBase
    {
        private readonly ISelectionProvider selectionProvider;

        private ObservableAsPropertyHelper<bool> _isSelected;
        public bool IsSelected
        {
            get => _isSelected.Value;
            set
            {
                try
                {
                    if (selectionProvider.Disconnected)
                        return;

                    if (value)
                    {
                        if (value != _isSelected.Value && !selectionProvider.SelectedItemIDs.Items.Contains(Item.ItemId))
                            selectionProvider.SelectedItemIDs.Add(Item.ItemId);
                    }
                    else
                    {
                        if (value != _isSelected.Value && selectionProvider.SelectedItemIDs.Items.Contains(Item.ItemId))
                            selectionProvider.SelectedItemIDs.Remove(Item.ItemId);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), $"{nameof(ItemSelectionInfo)}.{nameof(IsSelected)}.set");
                }
            }
        }
        public ItemEx Item { get; }
        public ItemSelectionInfo(ItemEx item, ISelectionProvider selectionProvider)
        {
            this.selectionProvider = selectionProvider;
            this.Item = item;
            this._isSelected =
                selectionProvider
                .SelectedItemIDs
                .Connect()
                .Filter(id => id == item.ItemId)
                .ToCollection()
                .Select(lst => lst.Any())
                .TakeUntil(destroy)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, nameof(IsSelected));
        }
    }

    public class ItemListViewModel : DisposableReactiveObjectBase, ISelectionProvider
    {
        private readonly IItemService _itemService;
        private readonly IInjectionProviderService _injectionProviderService;


        public BehaviorSubject<Func<ItemEx, bool>> FilterChanges { get; } = new BehaviorSubject<Func<ItemEx, bool>>(_ => true);
        ReadOnlyObservableCollection<ItemSelectionInfo> _items;
        public ReadOnlyObservableCollection<ItemSelectionInfo> Items => _items;
        public IObservableList<ItemEx> Selection { get; private set; }
        public ISourceList<ItemId> SelectedItemIDs { get; } = new SourceList<ItemId>();
        public bool Disconnected { get; private set; }

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

                var itemList = this._itemService
                    .GetExtended()
                    .Connect()
                    .Filter(FilterChanges)
                    .TakeUntil(destroy)
                    .Sort(item => item.Name);

                var selectionList =
                    itemList.Transform(item => new ItemSelectionInfo(item, this))
                    .DisposeMany();

                var _selection = 
                    SelectedItemIDs.Connect()
                    .AddKey(id => id)
                    .LeftJoin(
                        itemList,
                        item => item.ItemId,
                        (id, item) => new { id, item })
                    .RemoveKey();


                Selection = 
                    _selection
                    .Filter(x => x.item.HasValue)
                    .Transform(x => x.item.Value)
                    .AsObservableList();


                _selection
                     .Filter(x => !x.item.HasValue)
                     .ToCollection()
                     .Subscribe(col =>
                     {
                         if (col.Any())
                             SelectedItemIDs.RemoveMany(col.Select(x => x.id));
                     });

                selectionList
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Do(_ => this.Disconnected = true)
                    .Bind(out _items)
                    .Do(_ => this.Disconnected = false)
                    .Subscribe();

            });

        }




    }
}
