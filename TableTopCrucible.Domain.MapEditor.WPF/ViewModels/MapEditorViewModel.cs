using HelixToolkit.Wpf;

using MaterialDesignThemes.Wpf;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.WPF.PageViewModels;
using TableTopCrucible.Data.MapEditor.Stores;
using TableTopCrucible.Domain.MapEditor.Core.Layers;
using TableTopCrucible.Domain.MapEditor.Core.Managers;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.MapEditor.WPF.ViewModels
{
    public interface IMapEditorVm
    {
        IObservable<ItemId> SelectedItemIdChanges { get; set; }
    }
    public class MapEditorViewModel : DisposableReactiveObjectBase, IMapEditorVm
    {
        private readonly IMapDataService _mapDataService;
        private readonly IMapManager _mapManager;

        public HelixViewport3D Viewport { get; } = new HelixViewport3D();
        [Reactive]
        public IObservable<ItemId> SelectedItemIdChanges { get; set; }

        public MapEditorViewModel(IMapDataService mapDataService, IMapManager mapManager)
        {
            _mapDataService = mapDataService;
            _mapManager = mapManager;
            Viewport.Children.Add(new DefaultLights(), mapManager.MasterModel);

            var map = this._mapManager.CreateMap();

            mapManager.SelectedItemIdChanges = this.WhenAnyObservable(vm => vm.SelectedItemIdChanges);
        }

    }
}
