using System;

namespace TableTopCrucible.Domain.Models.ValueTypes.IDs
{
    public interface ITypedId
    {
        Guid ToGuid();
    }
}
