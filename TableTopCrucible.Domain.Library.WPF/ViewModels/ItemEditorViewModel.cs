using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using TableTopCrucible.Domain.Models.Views;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public class ItemEditorViewModel : DisposableReactiveValidationObject<ItemEditorViewModel>
    {
        private BehaviorSubject<ItemId?> _selectedItemIdChanges = new BehaviorSubject<ItemId?>(null);
        public IObservable<ItemId?> SelectedItemIdChanges => _selectedItemIdChanges;
        public IObservable<ExtendedItem?> SelectedItemChanges { get; }
        public ObservableAsPropertyHelper<ExtendedItem?> _selectedItem;
        public ExtendedItem? SelectedItem => _selectedItem.Value;
        private readonly IItemService _itemService;


        public TagEditorViewModel TagEdiotr { get; }


        [Reactive] public string Name { get; set; }
        [Reactive] public string Thumbnail { get; set; }
        [Reactive] public IEnumerable<ExtendedFileInfo> Files { get; set; }




        public ItemEditorViewModel(TagEditorViewModel tagEdiotr, IItemService itemService)
        {
            this.TagEdiotr = tagEdiotr;
            this.TagEdiotr.IsEditmode = true;
            this._itemService = itemService;
            this.disposables.Add(_selectedItemIdChanges);

            this.SelectedItemChanges = 
                this._itemService
                .GetExtended(this.SelectedItemIdChanges)
                .TakeUntil(destroy);
            this._selectedItem = SelectedItemChanges
                .ToProperty(this, nameof(SelectedItem));


            this.SelectedItemChanges.Subscribe(LoadItem);
        }


        public void SelectItem(ItemId? id)
            => this._selectedItemIdChanges.OnNext(id);

        public void LoadItem(ExtendedItem? item)
        {
            this.Name = (string)item?.Name;
            this.Thumbnail = (string)item?.Thumbnail;
            this.TagEdiotr.Tags = item?.Tags;
            this.Files = item?.Files;
        }
    }
}
