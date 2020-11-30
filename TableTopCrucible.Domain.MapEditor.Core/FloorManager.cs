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
using TableTopCrucible.Data.MapEditor.Stores;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.MapEditor.Core.Layers;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.MapEditor.Core
{
    public interface IFloorManager
    {
        public Visual3D MasterModel { get; }
        public ItemId SelectedItemId { get; set; }
    }
    public class FloorManager : DisposableReactiveObjectBase, IFloorManager
    {
        public ModelVisual3D masterModel = new ModelVisual3D();
        public Visual3D MasterModel => masterModel;
        public IGridLayer GridLayer { get; }
        public ITileLayer TileLayer { get; }

        [Reactive]
        public FloorId FloorId { get; set; }
        [Reactive]
        public ItemId SelectedItemId { get; set; }

        public FloorManager(IGridLayer gridLayer, ITileLayer tileLayer, IFloorDataService floorDataService, IItemDataService itemDataService)
        {
            this.GridLayer = gridLayer;
            TileLayer = tileLayer;

            masterModel.Children.Add(TileLayer.MasterModel, GridLayer.MasterModel);

            this.WhenAnyValue(vm => vm.FloorId)
                .Subscribe(id =>
                {
                    TileLayer.FloorId = id;
                    GridLayer.FloorId = id;
                });

            floorDataService.Get(
                this.WhenAnyValue(vm => vm.FloorId)
                .Skip(1))
                .Subscribe(floor => {
                    tileLayer.FloorId = floor.Id;
                });

            var itemChanges =
                this.WhenAnyValue(vm => vm.SelectedItemId)
                .Select(id => itemDataService.GetExtended(id))
                .Switch();

            Observable.CombineLatest(
                GridLayer.FieldMouseEnter,
                itemChanges,
                (location, item) => { return new { location, item }; })
                .TakeUntil(destroy)
                .Subscribe(x =>
                {

                });


        }
    }
}
