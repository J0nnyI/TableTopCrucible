using DynamicData;

using MaterialDesignThemes.Wpf;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms.VisualStyles;

using TableTopCrucible.Core.WPF.PageViewModels;
using TableTopCrucible.Domain.Library.WPF.ViewModels;
using TableTopCrucible.Domain.Library.WPF.Views;

namespace TableTopCrucible.Domain.Library.WPF.PageViewModels
{
    public class ItemEditorPageViewModel : PageViewModelBase
    {
        public ItemListViewModel ItemList { get; }
        public ItemEditorViewModel ItemEditor { get; }
        public ItemListFilterViewModel Filter { get; }

        public ItemEditorPageViewModel(ItemListViewModel itemList, ItemEditorViewModel itemEditor,
            ItemListFilterViewModel filter) : base("item Editor", PackIconKind.Edit)
        {
            this.ItemList = itemList;
            this.ItemEditor = itemEditor;
            filter.FilterChanges
                .TakeUntil(destroy)
                .Subscribe(itemList.FilterChanges.OnNext, ex => MessageBox.Show(ex.ToString()));
            Filter = filter;
            this.ItemList.SelectedItemChanges.Subscribe(x => itemEditor.SelectItem(x?.SourceItem.Id));
            itemEditor.SetTagpool(Filter.Tagpool.AsObservableList());
        }

    }
}
