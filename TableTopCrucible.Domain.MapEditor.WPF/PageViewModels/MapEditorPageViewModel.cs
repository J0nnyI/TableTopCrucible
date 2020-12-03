using DynamicData;

using HelixToolkit.Wpf;

using MaterialDesignThemes.Wpf;

using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.WPF.PageViewModels;
using TableTopCrucible.Data.Models.ValueTypes;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.MapEditor.Core.Services;
using TableTopCrucible.Domain.MapEditor.WPF.ViewModels;
using TableTopCrucible.FeatureCore.WPF.Tagging.ViewModels;
using TableTopCrucible.FeatureCore.WPF.ViewModels;

namespace TableTopCrucible.Domain.MapEditor.WPF.PageViewModels
{
    public interface IMapEditorPageVm:IPageViewModel
    {

    }
    public class MapEditorPageViewModel : PageViewModelBase, IMapEditorPageVm
    {
        [Reactive]
        public int CacheFileCount { get; private set; }
        public MapEditorPageViewModel(
            IMapEditorVm mapEditor,
            ItemListViewModel itemList,
            IManualTagEditor tagFilter,
            IItemDataService itemService,
            IModelCache modelCache
            ) : base("Map Editor", PackIconKind.Map)
        {
            MapEditor = mapEditor;
            ItemList = itemList;
            TagFilter = tagFilter;
            tagFilter.Editmode = true;
            tagFilter.PermitNewTags = false;
            tagFilter.SetTagpool(itemService.GetTags());

            modelCache.Get().CountChanged.Subscribe(count => this.CacheFileCount = count);

            tagFilter
                .Selection
                .Connect()
                .ToCollection()
                .ToFilter((ItemEx item, IReadOnlyCollection<Tag> tags) => item.Tags.ContainsAll(tags))
                .TakeUntil(destroy)
                .Subscribe(ItemList.FilterChanges.OnNext);

            mapEditor.SelectedItemIdChanges = 
                itemList
                    .SelectedItemIDs
                    .Connect()
                    .ToCollection()
                    .Select(lst => lst.FirstOrDefault());
        }

        public IMapEditorVm MapEditor { get; }
        public ItemListViewModel ItemList { get; }
        public IManualTagEditor TagFilter { get; }
    }
}

