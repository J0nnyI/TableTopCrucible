using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;
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

        public ItemEditorPageViewModel(ItemListViewModel itemList, ItemEditorViewModel itemEditor) : base("item Editor", PackIconKind.Edit)
        {
            this.ItemList = itemList;
            this.ItemEditor = itemEditor;

            this.ItemList.SelectedItemChanges.Subscribe(x => itemEditor.SelectItem(x?.SourceItem.Id));
        }

    }
}
