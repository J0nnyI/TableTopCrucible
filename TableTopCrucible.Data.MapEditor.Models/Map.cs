
using System;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Models.IDs;

namespace TableTopCrucible.Data.MapEditor.Models
{
    public struct Map : IEntity<MapId>
    {
        public MapId Id { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }
    }
}
