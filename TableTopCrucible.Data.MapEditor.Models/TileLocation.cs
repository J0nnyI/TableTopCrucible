using System;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.MapEditor.Models
{
    public struct TileLocation : IEntity<TileLocationId>
    {
        public TileLocationId Id { get; }
        public ItemId Item { get; }
        public TileLayerId Layer { get; }
        public int PosX { get; }
        public int PosY { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }
    }
}
