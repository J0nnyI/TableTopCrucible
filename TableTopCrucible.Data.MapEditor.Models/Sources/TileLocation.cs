using System;
using System.Drawing;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.MapEditor.Models.Sources
{
    public struct TileLocation : IEntity<TileLocationId>
    {
        public TileLocationId Id { get; }
        public ItemId ItemId { get; }
        public FloorId FloorId { get; }
        public Point Location { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }
    }
}
