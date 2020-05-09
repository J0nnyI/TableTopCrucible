using System;
using System.Collections.Generic;
using System.Text;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.ValueTypes;
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
