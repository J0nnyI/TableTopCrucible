using DynamicData;

using HelixToolkit.Wpf;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Media.Media3D;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Data.MapEditor.Models.Sources;
using TableTopCrucible.Data.MapEditor.Stores;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.MapEditor.Core.Models.Views;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.MapEditor.Core.Services
{
    public interface IModelCache
    {
        void AddMapToCache(MapId mapId);
        IObservableCache<Model3DGroup, ItemId> Get();
        IObservable<Model3DGroup> Get(IObservable<ItemId> idChanges);
        IObservable<Model3DGroup> Get(ItemId id);
        void RemoveMapFromCache(MapId mapId);
    }
    public class ModelCache:DisposableReactiveObjectBase, IModelCache
    {
        public IObservableCache<Model3DGroup, ItemId> Get() => _modelCache;
        private readonly IObservableCache<Model3DGroup, ItemId> _modelCache;
        private readonly IItemDataService _itemDataService;
        private ModelImporter _modelImporter = new ModelImporter()
        {
            DefaultMaterial = Materials.LightGray
        };
        private readonly ISourceList<MapId> _cachedMaps = new SourceList<MapId>();
        public ModelCache(IItemDataService itemDataService, IFloorDataService floorDataService, ITileLocationDataService tileLocationDataService)
        {

            _modelCache = _cachedMaps
                .Connect()
                .AddKey(id => id)
                .InnerJoinMany(
                    floorDataService
                        .Get()
                        .Connect(),
                    floor => floor.MapId,
                    (mapId, floors) => floors.Items.Select(floor => floor.Id)
                )
                .TransformMany(floors => floors, id => id)
                .InnerJoinMany(
                    tileLocationDataService
                        .Get()
                        .Connect(),
                    location => location.FloorId,
                    (floorId, locations) => locations.Items.Select(location => location.ItemId)
                )
                .TransformMany(locations => locations, id => id)
                .InnerJoin(
                    itemDataService.GetExtended().Connect(),
                    item => item.ItemId,
                    (_, item) => item
                )
                .Transform(_itemToModel)
                .AsObservableCache();

            _itemDataService = itemDataService;

        }

        public void AddMapToCache(MapId mapId) => _cachedMaps.Add(mapId);
        public void RemoveMapFromCache(MapId mapId) => _cachedMaps.Remove(mapId);

        public IObservable<Model3DGroup> Get(IObservable<ItemId> idChanges)
        {
            return idChanges
                .Select(id =>
                    this.Get()
                        .Connect()
                        .WatchValue(id)
                        .Select(model =>
                        {
                            return new { id, model };
                        })
                    )
                .Switch()
                .Select((x) =>
                {
                    if (x.model == null)
                        return this._itemDataService.GetExtended(x.id).Select(_itemToModel);
                    return Observable.Return(x.model);
                })
                .Switch();
        }
        public IObservable<Model3DGroup> Get(ItemId id)
        {
            return this
                .Get()
                .Connect()
                .WatchValue(id)
                .Select(model =>
                {
                    if (model == null)
                        return this._itemDataService.GetExtended(id).Select(_itemToModel);
                    return Observable.Return(model);
                })
                .Switch();
        }
        private Model3DGroup _itemToModel(ItemEx item)
        {
            var model = _modelImporter.Load(item.LatestFilePath);
            model.PlaceAtOrigin();
            return model;
        }
    }
}
