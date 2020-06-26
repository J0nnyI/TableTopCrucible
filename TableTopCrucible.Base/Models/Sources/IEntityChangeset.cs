using System.Collections.Generic;

using TableTopCrucible.Base.Models.ValueTypes.IDs;

namespace TableTopCrucible.Base.Models.Sources
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
