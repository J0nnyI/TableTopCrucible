﻿using DynamicData;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Media.Media3D;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Data.MapEditor.Models.Sources;
using TableTopCrucible.Data.MapEditor.Stores;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.MapEditor.Core.Layers;
using TableTopCrucible.Domain.MapEditor.Core.Models;
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
        IObservable<RotationDirection> OnModelRotation { get; set; }
    }
    public class MapManager : DisposableReactiveObjectBase, IMapManager
    {
        private ContainerUIElement3D masterModel = new ContainerUIElement3D();
        public Visual3D MasterModel => masterModel;
        private readonly IGridLayer _gridLayer;
        private readonly IMapDataService _mapDataService;
        private readonly IFloorDataService _floorDataService;
        private readonly ICursorManager _cursorManager;
        private readonly IModelCache _modelCache;
        private readonly ISelectionManager _selectionManager;

        [Reactive]
        public IObservable<ItemId> SelectedItemIdChanges { get; set; }
        [Reactive]
        public FloorId FloorId { get; set; }


        [Reactive]
        public MapId MapId { get; set; }
        [Reactive]
        public IObservable<RotationDirection> OnModelRotation { get; set; }

        public MapManager(
            IGridLayer gridLayer,
            IMapDataService mapDataService,
            IFloorDataService floorDataService,
            IItemDataService itemDataService,
            IMapEditorManagementService mapEditorManagementService,
            ICursorManager cursorManager,
            IModelCache modelCache,
            ITileLocationDataService tileLocationDataService,
            IInjectionProviderService injectionProviderService,
            ISelectionManager selectionManager)
        {

            _gridLayer = gridLayer;
            _mapDataService = mapDataService;
            _floorDataService = floorDataService;
            _cursorManager = cursorManager;
            _modelCache = modelCache;
            _selectionManager = selectionManager;
            handleCaching();

            var floorIdChanges = this.WhenAnyValue(vm => vm.FloorId);
            floorIdChanges.BindTo(this, vm => vm._gridLayer.FloorId);

            var itemIdChanges = this.WhenAnyObservable(vm => vm.SelectedItemIdChanges);



            cursorManager.OnModelRotation = this.WhenAnyObservable(vm => vm.OnModelRotation);

            var floors =
                this._floorDataService.Get()
                .Connect()
                .Filter(floorIdChanges.ToFilter((Floor floor, FloorId id) => floor.Id == id))
                .Transform(floor =>
                {
                    var floorMgr = injectionProviderService.Provider.GetRequiredService<IFloorManager>();

                    floorMgr.FloorId = floor.Id;
                    this.masterModel.Children.Add(floorMgr.MasterModel);

                    return floorMgr;
                })
                .AsObservableCache();

            floors.Connect().OnItemRemoved(floorMgr => masterModel.Children.Remove(floorMgr.MasterModel));
            this.WhenAnyValue(vm => vm.FloorId).BindTo(this, vm => vm._selectionManager.SelectedFloor);
            this.WhenAnyObservable(vm => vm.SelectedItemIdChanges)
                .Select(id=>itemDataService.GetExtended(id))
                .Switch()
                .BindTo(this, vm => vm._selectionManager.SelectedItem);


            masterModel.Children.Add(_gridLayer.MasterModel, _cursorManager.MasterModel);
        }

        private void handleCaching()
        {
            this.WhenAnyValue(vm => vm.MapId)
                .Pairwise()
                .TakeUntil(Destroy)
                .Subscribe(
                    change =>
                    {
                        if (change.Previous.HasValue && change.Previous.Value != default)
                            _modelCache.RemoveMapFromCache(change.Previous.Value);
                        _modelCache.AddMapToCache(change.Current.Value);
                    },
                    ex => { },
                    () => _modelCache.RemoveMapFromCache(this.MapId));
        }

        public Map CreateMap()
        {
            var map = new Map("new Map", string.Empty);
            _mapDataService.Post(map);

            var floor = new Floor(string.Empty, map.Id, 0);
            this.FloorId = floor.Id;
            _floorDataService.Post(floor);

            MapId = map.Id;
            return map;
        }
        public void OpenMap(MapId mapId)
        {
            MapId = mapId;
        }
    }
}
