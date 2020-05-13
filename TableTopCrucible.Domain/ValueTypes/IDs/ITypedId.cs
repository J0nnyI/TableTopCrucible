using System;

namespace TableTopCrucible.Domain.ValueTypes.IDs
{
    public interface ITypedId
    {
        Guid ToGuid();
    }
}
