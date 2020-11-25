using System;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Models.IDs;

namespace TableTopCrucible.Data.MapEditor.Models.Sources
{
    public struct Floor : IEntity<FloorId>
    {
        public Floor(TileLayer? tileLayer, MapId map, int height)
        {
            TileLayer = tileLayer;
            Id = FloorId.New();
            Map = map;
            LastChange = Created = DateTime.Now;
            Height = height;
        }

        public TileLayer? TileLayer { get; }
        public FloorId Id { get; }
        public MapId Map { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }
        public int Height { get; }
    }
}
