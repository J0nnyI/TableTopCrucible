using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Media.Media3D;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Data.MapEditor.Models.Sources;
using TableTopCrucible.Data.MapEditor.Stores;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.MapEditor.Core.Layers;
using TableTopCrucible.Domain.MapEditor.Core.Services;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.MapEditor.Core.Managers
{
    public interface IMapManager
    {
        Visual3D MasterModel { get; }
        Map CreateMap();
        void OpenMap(MapId mapId);

        public IObservable<ItemId> SelectedItemIdChanges { get; set; }
    }
    public class MapManager : DisposableReactiveObjectBase, IMapManager
    {
        private ModelVisual3D masterModel = new ModelVisual3D();
        public Visual3D MasterModel => masterModel;
        private readonly IGridLayer _gridLayer;
        private readonly IMapDataService _mapDataService;
        private readonly IFloorDataService _floorDataService;

        [Reactive]
        public IObservable<ItemId> SelectedItemIdChanges { get; set; }


        [Reactive]
        public MapId MapId { get; set; }
        public MapManager(IGridLayer gridLayer, IMapDataService mapDataService, IFloorDataService floorDataService, IItemDataService itemDataService, IMapEditorManagementService mapEditorManagementService, IModelCache modelCache)
        {


            _gridLayer = gridLayer;
            _mapDataService = mapDataService;
            _floorDataService = floorDataService;

            this.WhenAnyValue(vm => vm.MapId)
                .PreviousAndCurrent()
                .TakeUntil(destroy)
                .Subscribe(
                    change =>
                    {
                        if(change.Previous.HasValue && change.Previous.Value != default)
                            modelCache.RemoveMapFromCache(change.Previous.Value);
                        modelCache.AddMapToCache(change.Current.Value);
                    },
                    () => modelCache.RemoveMapFromCache(this.MapId));

            var itemIDChanges = this.WhenAnyObservable(vm => vm.SelectedItemIdChanges);

            var cursorModel = modelCache
                .Get(itemIDChanges)
                .CombineLatest(
                    gridLayer.FieldMouseEnter,
                    (model, location) => { return new { model, location }; });


            masterModel.Children.Add(_gridLayer.MasterModel);
        }

        public Map CreateMap()
        {
            var map = new Map("new Map", string.Empty);
            _mapDataService.Post(map);

            var floor = new Floor(null, map.Id, 0);
            _floorDataService.Post(floor);
            _gridLayer.Init(floor.Id, 51);

            MapId = map.Id;
            return map;
        }
        public void OpenMap(MapId mapId)
        {
            MapId = mapId;
        }
    }
}
