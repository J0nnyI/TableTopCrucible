using DynamicData;

using MaterialDesignThemes.Wpf;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms.VisualStyles;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.WPF.PageViewModels;
using TableTopCrucible.Domain.Library.WPF.ViewModels;
using TableTopCrucible.Domain.Library.WPF.Views;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Library.WPF.PageViewModels
{
    public class ItemEditorPageViewModel : PageViewModelBase
    {
        public ItemListViewModel ItemList { get; }
        private ItemEditorViewModel _itemEditor { get; }
        private IMultiItemEditor _multiItemEditor { get; }
        [Reactive]
        public object ItemEditor { get; set; }
        public ItemListFilterViewModel Filter { get; }

        public ItemEditorPageViewModel(
            ItemListViewModel itemList,
            ItemEditorViewModel itemEditor,
            IMultiItemEditor multiItemEditor,
            ItemListFilterViewModel filter) : base("item Editor", PackIconKind.Edit)
        {
            this.ItemList = itemList;
            this.ItemEditor = this._itemEditor = itemEditor;
            this._multiItemEditor = multiItemEditor;
            this.Filter = filter;

            multiItemEditor.BindSelection(itemList.Selection);

            this.disposables.Add(ItemList, (IDisposable)ItemEditor, _multiItemEditor, Filter);

            filter.FilterChanges
                .TakeUntil(destroy)
                .Subscribe(itemList.FilterChanges.OnNext, ex => MessageBox.Show(ex.ToString()));

            this.ItemList.Selection.Connect()
                .ToCollection()
                .Subscribe(x =>
                {
                    if (x.Count <= 1)
                    {
                        ItemEditor = _itemEditor;
                        itemEditor.SelectItem(x.Any() ? (ItemId?)x.FirstOrDefault().SourceItem.Id : null);
                    }
                    else
                    {
                        ItemEditor = _multiItemEditor;
                    }
                });
            itemEditor.SetTagpool(Filter.Tagpool.AsObservableList());
            multiItemEditor.SetTagpool(Filter.Tagpool.AsObservableList());
        }

    }
}
