using System;

using TableTopCrucible.Domain.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models
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
