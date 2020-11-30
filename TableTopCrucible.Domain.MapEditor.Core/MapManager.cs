using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Data.MapEditor.Models.Sources;
using TableTopCrucible.Data.MapEditor.Stores;
using TableTopCrucible.Domain.MapEditor.Core.Layers;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.MapEditor.Core
{
    public interface IMapManager
    {
        Visual3D MasterModel { get; }
        IObservable<MapId> MapIdChanges { get; set; }
        public IObservable<ItemId> SelectedItemIdChanges { get; set; }
    }
    public class MapManager : DisposableReactiveObjectBase,IMapManager
    {
        private ModelVisual3D masterModel = new ModelVisual3D();
        public Visual3D MasterModel => masterModel;
        private readonly IGridLayer _gridLayer;
        private readonly IMapDataService _mapDataService;

        [Reactive]
        public IObservable<ItemId> SelectedItemIdChanges { get; set; }


        [Reactive]
        public IObservable<MapId> MapIdChanges { get; set; }
        public MapManager(IGridLayer gridLayer, IMapDataService mapDataService)
        {
            _gridLayer = gridLayer;
            _mapDataService = mapDataService;
            Observable.CombineLatest(
                gridLayer.FieldMouseEnter,
                this.WhenAnyObservable(vm=>vm.SelectedItemIdChanges),
                (location, item) => { return new { location, item }; })
                .TakeUntil(destroy)
                .Subscribe(x =>
                {

                });
        }

        public void OpenNewMap()
        {
            var map = new Map("new Map",string.Empty);
            this._mapDataService.Post(map);
        }
    }
}
