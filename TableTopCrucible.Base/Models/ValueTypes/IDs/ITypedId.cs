using System;

namespace TableTopCrucible.Base.Models.ValueTypes.IDs
{
    public interface ITypedId
    {
        Guid ToGuid();
    }
}
