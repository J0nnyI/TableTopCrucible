using DynamicData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Data.MapEditor.Models.Changesets;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Data.MapEditor.Models.Sources;
using TableTopCrucible.Data.Services;

namespace TableTopCrucible.Data.MapEditor.Stores
{
    public interface ITileLocationDataService : IDataService<TileLocation, TileLocationId, TileLocationChangeset>
    {
        IObservable<Rect> SizeByFloor(IObservable<FloorId> layer);
    }
    public class TileLocationDataService : DataServiceBase<TileLocation, TileLocationId, TileLocationChangeset>, ITileLocationDataService
    {
        public TileLocationDataService(ISettingsService settingsService, INotificationCenterService notificationCenter) : base(settingsService, notificationCenter)
        {
        }
        public IObservable<Rect> SizeByFloor(IObservable<FloorId> floorId)
        {
            return this
                .Get()
                .Connect()
                .RemoveKey()
                .Filter(floorId.ToFilter((TileLocation location, FloorId id) => location.FloorId == id))
                .QueryWhenChanged(locations =>
                {
                    return new Rect(
                        new Point(
                            locations.Min(loc => loc.Location.X),
                            locations.Min(loc => loc.Location.Y)
                        ),
                        new Point(
                            locations.Max(loc => loc.Location.X),
                            locations.Max(loc => loc.Location.Y)
                        )
                    );
                })
                .StartWith(new Rect(0,0,0,0))
                .DistinctUntilChanged();

        }
    }
}
