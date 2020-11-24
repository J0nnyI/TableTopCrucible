using System;
using System.Collections.Generic;

using TableTopCrucible.Core.Models.ValueTypes;
using TableTopCrucible.Data.MapEditor.Models.IDs;

namespace TableTopCrucible.Data.MapEditor.Models.Changesets
{
    public class TileLocationChangeset : IEntityChangeset<TileLocation, TileLocationId>
    {
        public TileLocation? Origin { get; }

        public TileLocation Apply()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetErrors()
        {
            throw new NotImplementedException();
        }

        public TileLocation ToEntity()
        {
            throw new NotImplementedException();
        }
    }
}
