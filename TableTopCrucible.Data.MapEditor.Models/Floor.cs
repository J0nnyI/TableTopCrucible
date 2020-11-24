using System;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Models.IDs;

namespace TableTopCrucible.Data.MapEditor.Models
{
    public struct Floor : IEntity<FloorId>
    {
        public FloorId Id { get; }
        public MapId Map { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }
    }
}
