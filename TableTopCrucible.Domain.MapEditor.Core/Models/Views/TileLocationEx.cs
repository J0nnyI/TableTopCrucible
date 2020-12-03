using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Data.MapEditor.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.MapEditor.Core.Models.Views
{
    public struct TileLocationEx
    {
        public TileLocationEx(Floor floor, TileLocation location)
        {
            Floor = floor;
            Location = location;
        }

        public Floor Floor { get; }
        public TileLocation Location { get; }

        public Point3D Origin
            => new Point3D(Location.Location.X, Location.Location.Y, Floor.Height);
        public TileLocationId LocationId
            => Location.Id;
        public ItemId ItemId
            => Location.ItemId;
        public FloorId FloorId
            => Floor.Id;
    }
}
