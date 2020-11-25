using System;
using System.Collections.Generic;
using System.Text;
using TableTopCrucible.Data.MapEditor.Models.Sources;

namespace TableTopCrucible.Data.MapEditor.Models.Views
{
    public class FloorEx
    {
        public FloorEx(Floor floor, TileLayerEx? tileLayer)
        {
            Floor = floor;
            TileLayer = tileLayer;
        }

        public Floor Floor { get; }
        public TileLayerEx? TileLayer { get; }
    }
}
