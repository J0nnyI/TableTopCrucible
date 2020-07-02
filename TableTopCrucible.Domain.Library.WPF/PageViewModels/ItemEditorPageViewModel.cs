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
        public ItemEditorViewModel ItemEdior { get; }

        public ItemEditorPageViewModel(ItemListViewModel itemList, ItemEditorViewModel itemEdior) : base("item Editor", PackIconKind.Edit)
        {
            this.ItemList = itemList;
            this.ItemEdior = itemEdior;
        }

    }
}
