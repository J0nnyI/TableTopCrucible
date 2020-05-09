using System;
using System.Collections.Generic;
using System.Text;

namespace TableTopCrucible.Domain.ValueTypes.IDs
{
    public interface ITypedId
    {
        Guid ToGuid();
    }
}
