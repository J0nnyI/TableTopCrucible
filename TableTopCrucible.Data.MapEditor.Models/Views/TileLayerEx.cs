using System;
using System.Collections.Generic;
using System.Text;
using TableTopCrucible.Data.MapEditor.Models.Sources;

namespace TableTopCrucible.Data.MapEditor.Models.Views
{
    public struct TileLayerEx
    {
        public TileLayerEx(TileLayer tileLayer, IEnumerable<TileLocations> tileLocations)
        {
            TileLayer = tileLayer;
            TileLocations = tileLocations;
        }

        public TileLayer TileLayer { get; }
        public IEnumerable<TileLocations> TileLocations { get; }
    }
}
