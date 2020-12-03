using System;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Models.IDs;

namespace TableTopCrucible.Data.MapEditor.Models.Sources
{
    public struct Floor : IEntity<FloorId>
    {
        public Floor(string name,MapId mapId, int height)
        {
            this.Name = name;
            Id = FloorId.New();
            MapId = mapId;
            LastChange = Created = DateTime.Now;
            Height = height;
        }
        public string Name { get; }
        public FloorId Id { get; }
        public MapId MapId { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }
        public int Height { get; }
    }
}
