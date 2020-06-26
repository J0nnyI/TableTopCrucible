using System.Collections.Generic;

using TableTopCrucible.Core.Models.ValueTypes.IDs;

namespace TableTopCrucible.Core.Models.Sources
{
    public interface IEntityChangeset<Tentity, Tid>
        where Tentity : struct, IEntity<Tid>
        where Tid : ITypedId
    {
        Tentity? Origin { get; }
        Tentity ToEntity();
        Tentity Apply();
        IEnumerable<string> GetErrors();
    }
}
