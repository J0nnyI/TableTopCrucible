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
    public interface IFloorDataService: IDataService<Floor, FloorId, FloorChangeset>
    {

    }
    public class FloorDataService : DataServiceBase<Floor, FloorId, FloorChangeset>, IFloorDataService
    {
        private readonly ITileLocationDataService tileLocationDataService;

        public FloorDataService(ISettingsService settingsService, INotificationCenterService notificationCenter, ITileLocationDataService tileLocationDataService) : base(settingsService, notificationCenter)
        {
            this.tileLocationDataService = tileLocationDataService;
            
        }

    }
}
