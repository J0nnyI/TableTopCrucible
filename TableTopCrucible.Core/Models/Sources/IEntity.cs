using System;

using TableTopCrucible.Core.Models.ValueTypes.IDs;

namespace TableTopCrucible.Core.Models.Sources
{
    public interface IEntity<Tid> : IEntity
        where Tid : ITypedId
    {
        new Tid Id { get; }
        Guid IEntity.Id => Id.ToGuid();
    }

    public interface IEntity
    {
        Guid Id { get; }
        DateTime Created { get; }
        DateTime LastChange { get; }
    }
}
