
using System;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Models.IDs;

namespace TableTopCrucible.Data.MapEditor.Models
{
    public struct TileLayer : IEntity<TileLayerId>
    {
        public TileLayerId Id { get; }
        public FloorId Floor { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }
    }
}
