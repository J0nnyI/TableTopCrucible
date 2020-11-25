using System;
using System.Collections.Generic;

using TableTopCrucible.Core.Models.ValueTypes;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Data.MapEditor.Models.Sources;

namespace TableTopCrucible.Data.MapEditor.Models.Changesets
{
    public class MapChangeset : IEntityChangeset<Map, MapId>
    {
        public Map? Origin { get; }

        public Map Apply()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetErrors()
        {
            throw new NotImplementedException();
        }

        public Map ToEntity()
        {
            throw new NotImplementedException();
        }
    }
}
