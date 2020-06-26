using System;

namespace TableTopCrucible.Core.Models.ValueTypes.IDs
{
    public interface ITypedId
    {
        Guid ToGuid();
    }
}
