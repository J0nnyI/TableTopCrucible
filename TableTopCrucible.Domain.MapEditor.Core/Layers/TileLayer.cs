using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Stores;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.MapEditor.Core.Layers
{
    public class TileLayer : DisposableReactiveObjectBase
    {
        private readonly TileLocationDataService tileLocationDataService;

        

        public TileLayer(TileLocationDataService tileLocationDataService)
        {
            this.tileLocationDataService = tileLocationDataService;
            
        }

        public void PlaceItem(ItemId item, Rect3D slot)
        {

        }
    }
}
