using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using TableTopCrucible.Domain.Models.Views;
using TableTopCrucible.WPF.Commands;

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
        public FileVersionListViewModel FileVersionList { get; }

        [Reactive] public string Name { get; set; }
        [Reactive] public string Thumbnail { get; set; }
        [Reactive] public IEnumerable<VersionedFile> Files { get; set; }

        public ICommand Save { get; }


        public ItemEditorViewModel(
            TagEditorViewModel tagEdiotr,
            IItemService itemService,
            FileVersionListViewModel fileVersionList)
        {
            this.TagEdiotr = tagEdiotr;
            this.TagEdiotr.IsEditmode = true;
            this._itemService = itemService;
            FileVersionList = fileVersionList;
            this.disposables.Add(_selectedItemIdChanges);

            this.SelectedItemChanges = 
                this._itemService
                .GetExtended(this.SelectedItemIdChanges)
                .TakeUntil(destroy);
            this._selectedItem = SelectedItemChanges
                .ToProperty(this, nameof(SelectedItem));

            this.Save = new RelayCommand(_ => _save());

            this.SelectedItemChanges.Subscribe(LoadItem);
        }

        private void _save()
        {
            var cs = new ItemChangeset(SelectedItem.Value.Item);
            cs.Name = this.Name;
            cs.Thumbnail = this.Thumbnail;
            cs.Tags = this.TagEdiotr.Tags;
            this._itemService.Patch(cs);
        }
        public void SelectItem(ItemId? id)
            => this._selectedItemIdChanges.OnNext(id);

        public void LoadItem(ExtendedItem? item)
        {
            this.Name = (string)item?.Name;
            this.Thumbnail = (string)item?.Thumbnail;
            this.FileVersionList.SetFiles(item?.Files);
            this.TagEdiotr.Tags = item?.Tags;
            this.Files = item?.Files;
        }
    }
}
