using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TableTopCrucible.Data.Models.Views;

namespace TableTopCrucible.Data.MapEditor.Models.Views
{
    public struct TileLocations
    {
        public TileLocations(ItemEx item, IEnumerable<Point> locations)
        {
            Item = item;
            Locations = locations;
        }

        public ItemEx Item { get; }
        public IEnumerable<Point> Locations { get; }
    }
}
