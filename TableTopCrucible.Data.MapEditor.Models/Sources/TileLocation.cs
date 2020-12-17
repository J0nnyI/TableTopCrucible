using System;
using System.Windows;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.MapEditor.Models.Sources
{
    public struct TileLocation : IEntity<TileLocationId>
    {
        public TileLocation(ItemId itemId, FloorId floorId, Point location, double horizontalRotation)
        {
            ItemId = itemId;
            FloorId = floorId;
            Location = location;
            HorizontalRotation = horizontalRotation;
            Id = TileLocationId.New();
            LastChange = Created = DateTime.Now;
        }

        public TileLocationId Id { get; }
        public ItemId ItemId { get; }
        public FloorId FloorId { get; }
        public Point Location { get; }
        public double HorizontalRotation { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }
    }
}
