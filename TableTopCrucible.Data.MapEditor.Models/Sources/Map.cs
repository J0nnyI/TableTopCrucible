
using System;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Models.IDs;

namespace TableTopCrucible.Data.MapEditor.Models.Sources
{
    public struct Map : IEntity<MapId>
    {
        public Map(string name, string description) : this()
        {
            Id = MapId.New();
            LastChange = Created = DateTime.Now;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        public MapId Id { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }
        public string Name { get; }
        public string Description { get; }
    }
}
