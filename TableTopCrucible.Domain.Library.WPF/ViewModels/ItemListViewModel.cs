
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
using System.Windows.Input;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public struct ItemClickedEventArgs
    {
        public ItemClickedEventArgs(ItemSelectionInfo item, MouseButtonEventArgs e)
        {
            Item = item;
            EventArgs = e;
        }

        public ItemSelectionInfo Item { get; }
        public MouseButtonEventArgs EventArgs { get; }
    }
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
        public ICommand ItemClickedCommand { get; }
        public bool Disconnected { get; private set; }
        public ICommand DeselectAllCommand { get; }
        public ICommand ListKeyUpCommand { get; }
        public ItemListViewModel(
            IItemService itemService,
            IInjectionProviderService injectionProviderService)
        {
            this._itemService = itemService ?? throw new NullReferenceException("got no itemService");
            this._injectionProviderService = injectionProviderService ?? throw new NullReferenceException("got no itemservice");

            this.DeselectAllCommand = new RelayCommand(
                _ => this.SelectedItemIDs.Clear(),
                _ => this.SelectedItemIDs.Items.Any());

            this.ItemClickedCommand = new RelayCommand(onItemClicked);

            this.ListKeyUpCommand = new RelayCommand(onListKeyUp);

            this._injectionProviderService.Provider.Subscribe(
                (provider) =>
            {
                if (provider == null)
                    throw new InvalidOperationException("provider is null");
                #region list assembly
                var itemList = this._itemService
                    .GetExtended()
                    .Connect()
                    .Filter(FilterChanges)
                    .TakeUntil(destroy);

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
                    .Sort(item => item.Item.Name)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Do(_ => this.Disconnected = true)
                    .Bind(out _items)
                    .Do(_ => this.Disconnected = false)
                    .Subscribe();
                #endregion
            });



        }

        void onItemClicked(object e)
        {
            if (e is ItemClickedEventArgs args)
                onItemClicked(args);
            else
                throw new InvalidOperationException($"{nameof(ItemListViewModel)}.{nameof(onItemClicked)} invalid args {e}");
        }

        private bool isKeyPressed(ModifierKeys key)
         => isKeyPressed(Keyboard.Modifiers, key);
        private bool isKeyPressed( ModifierKeys pressedMods, ModifierKeys key)
         => ((pressedMods & key) == key);
        private ItemSelectionInfo previouslyClickedItem = null;
        void onItemClicked(ItemClickedEventArgs args)
        {
            var curItem = args.Item;
            var prevItem = previouslyClickedItem;
            var isStrgPressed = isKeyPressed(ModifierKeys.Control);
            var isShiftPressed = isKeyPressed(ModifierKeys.Shift);
            var isAltPressed = isKeyPressed(ModifierKeys.Alt);


            if (isStrgPressed)
            {
                curItem.IsSelected = !curItem.IsSelected;
            }
            else if (isShiftPressed)
            {
                var section = this.Items.Subsection(curItem, prevItem)
                    .ToList();
                var allSelected = section.All(item => item.IsSelected);
                foreach (var item in section)
                    item.IsSelected = !allSelected;
            }
            else
            {
                foreach(var item in Items.Except(curItem.AsArray()))
                    item.IsSelected = false;
                curItem.IsSelected = true;
            }
            previouslyClickedItem = curItem;
        }

        void onListKeyUp(object e)
        {
            if (e is KeyEventArgs args)
                onListKeyUp(args);
            else
                throw new InvalidOperationException($"{nameof(ItemListViewModel)}.{nameof(onListKeyUp)} invalid args {e}");
        }
        void onListKeyUp(KeyEventArgs args)
        {
            if(isKeyPressed(ModifierKeys.Control) && args.Key == Key.A)
            {
                var areAllSelected = Items.All(item => item.IsSelected);
                foreach (var item in Items)
                    item.IsSelected = !areAllSelected;
            }
        }
    }
}
