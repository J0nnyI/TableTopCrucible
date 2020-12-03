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
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Data.MapEditor.Stores;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.MapEditor.Core.Models.Views;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.MapEditor.Core.Services
{
    public interface IMapEditorManagementService
    {
        IObservable<IChangeSet<TileLocationEx, TileLocationId>> GetLocationEx();
    }
    public class MapEditorManagementService : IMapEditorManagementService
    {
        private readonly IObservable<IChangeSet<TileLocationEx, TileLocationId>> _getLocationEx;
        public IObservable<IChangeSet<TileLocationEx, TileLocationId>> GetLocationEx() => _getLocationEx;

        public MapEditorManagementService(IFloorDataService floorDataService, ITileLocationDataService tileLocationDataService)
        {
            _getLocationEx = floorDataService
                .Get()
                .Connect()
                .InnerJoinMany(
                    tileLocationDataService
                        .Get()
                        .Connect(),
                    location => location.FloorId,
                    (floor, tiles) => tiles?.Items?.Select(tile=> new TileLocationEx(floor, tile)))
                .TransformMany(tiles=>tiles, tile=>tile.Location.Id);
        }
      

    }
}
