using DynamicData;

using HelixToolkit.Wpf;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Data.MapEditor.Models.Sources;
using TableTopCrucible.Data.MapEditor.Stores;
using TableTopCrucible.Data.Services;

namespace TableTopCrucible.Domain.MapEditor.Core.Services
{
    public interface IModelCache
    {

    }
    public class ModelCache:DisposableReactiveObjectBase, IModelCache
    {
        private ModelImporter modelImporter = new ModelImporter()
        {
            DefaultMaterial = Materials.LightGray
        };
        [Reactive]
        public MapId MapId { get; set; }
        public ModelCache(IFloorDataService floorDataService, IItemDataService itemDataService, ITileLocationDataService tileLocationDataService)
        {
            var mapIdChanges = this.WhenAnyValue(vm => vm.MapId);

            var modelCache = 
                floorDataService
                .Get()
                .Connect()
                .Filter(mapIdChanges.ToFilter((Floor floor, MapId mapId) => floor.MapId == MapId))
                .InnerJoin(
                    tileLocationDataService
                        .Get()
                        .Connect(),
                    location => location.FloorId,
                    (floor, location) => location)
                .DistinctValues(location=>location.ItemId)
                .InnerJoin(
                    itemDataService.GetExtended().Connect(),
                    item=>item.ItemId,
                    (id, item)=>item)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Transform(item =>
                {
                    var model = modelImporter.Load(item.LatestFilePath);
                    model.PlaceAtOrigin();
                    model.Freeze();
                    return model;
                });

            
        }
    }
}
