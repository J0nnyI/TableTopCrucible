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
    public interface ITileLayerDataService:IDataService<TileLayer, TileLayerId, TileLayerChangeset>
    {

    }
    public class TileLayerDataService : DataServiceBase<TileLayer, TileLayerId, TileLayerChangeset>, ITileLayerDataService
    {
        protected TileLayerDataService(ISettingsService settingsService, INotificationCenterService notificationCenter) : base(settingsService, notificationCenter)
        {
        }
    }
}
