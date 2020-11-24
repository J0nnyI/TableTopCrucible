using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Core.Services;
using TableTopCrucible.Data.MapEditor.Models;
using TableTopCrucible.Data.MapEditor.Models.Changesets;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Data.Services;

namespace TableTopCrucible.Data.MapEditor.Stores
{
    public interface ITileLocationDataService : IDataService<TileLocation, TileLocationId, TileLocationChangeset>
    {

    }
    public class TileLocationDataService : DataServiceBase<TileLocation, TileLocationId, TileLocationChangeset>,ITileLocationDataService
    {
        protected TileLocationDataService(ISettingsService settingsService, INotificationCenterService notificationCenter) : base(settingsService, notificationCenter)
        {
        }
    }
}
