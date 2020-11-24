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
    public interface IMapDataService:IDataService<Map, MapId>
    {

    }
    public class MapDataService : DataServiceBase<Map, MapId, MapChangeset>, IMapDataService
    {
        protected MapDataService(ISettingsService settingsService, INotificationCenterService notificationCenter) : base(settingsService, notificationCenter)
        {
        }
    }
}
