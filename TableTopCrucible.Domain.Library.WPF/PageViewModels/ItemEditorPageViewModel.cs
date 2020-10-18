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
        public ItemEditorViewModel ItemEditor { get; }
        public IMultiItemEditor MultiItemEditor { get; }
        [Reactive]
        public object Editor { get; private set; }
        [Reactive]
        public bool IsSingle { get; private set; }
        public ItemListFilterViewModel Filter { get; }

        public ItemEditorPageViewModel(
            ItemListViewModel itemList,
            ItemEditorViewModel itemEditor,
            IMultiItemEditor multiItemEditor,
            ItemListFilterViewModel filter) : base("item Editor", PackIconKind.Edit)
        {
            this.ItemList = itemList;
            this.MultiItemEditor = multiItemEditor;
            Editor = this.ItemEditor = itemEditor;
            this.Filter = filter;

            multiItemEditor.BindSelection(itemList.Selection);

            this.disposables.Add(ItemList, ItemEditor, MultiItemEditor, Filter);

            filter.FilterChanges
                .TakeUntil(destroy)
                .Subscribe(itemList.FilterChanges.OnNext, ex => MessageBox.Show(ex.ToString()));

            this.ItemList.Selection.Connect()
                .ToCollection()
                .Subscribe(x =>
                {

                    this.IsSingle = x.Count <= 1;
                    if (x.Count == 1)
                        itemEditor.SelectItem(x.FirstOrDefault().SourceItem.Id);
                    else
                        itemEditor.SelectItem(null);

                    if (x.Count <= 1)
                        this.Editor = ItemEditor;
                    else
                        this.Editor = MultiItemEditor;
                });
            itemEditor.SetTagpool(Filter.Tagpool.AsObservableList());
            multiItemEditor.SetTagpool(Filter.Tagpool.AsObservableList());
        }

    }
}
