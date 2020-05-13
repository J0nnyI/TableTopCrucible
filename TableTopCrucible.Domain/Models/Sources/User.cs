using System;

using TableTopCrucible.Domain.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public struct User : IEntity<UserId>
    {
        public UserId Id { get; }
        public string Name { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }
    }
}
