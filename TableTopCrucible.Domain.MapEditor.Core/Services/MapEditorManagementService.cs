using DynamicData;
using System;
using System.Collections.Generic;
using System.Text;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Data.MapEditor.Stores;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.MapEditor.Core.Models.Views;

namespace TableTopCrucible.Domain.MapEditor.Core.Services
{
    public interface IMapEditorManagementService
    {
        IObservable<IChangeSet<TileLocationEx, FloorId>> GetLocationEx();
    }
    public class MapEditorManagementService:IMapEditorManagementService
    {
        private readonly IObservable<IChangeSet<TileLocationEx, FloorId>> _getLocationEx;
        public IObservable<IChangeSet<TileLocationEx, FloorId>> GetLocationEx() => _getLocationEx;

        public MapEditorManagementService(ITileLocationDataService tileLocationDataService, IFloorDataService floorDataService, IItemDataService itemDataService)
        {
            _getLocationEx = floorDataService
                .Get()
                .Connect()
                .InnerJoin(
                    tileLocationDataService
                        .Get()
                        .Connect(),
                    location => location.FloorId,
                    (floor, tile)=>new TileLocationEx(floor,tile));
        }
        
    }
}
