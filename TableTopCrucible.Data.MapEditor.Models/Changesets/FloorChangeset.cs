using System;
using System.Collections.Generic;

using TableTopCrucible.Core.Models.ValueTypes;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Data.MapEditor.Models.Sources;

namespace TableTopCrucible.Data.MapEditor.Models.Changesets
{
    public class FloorChangeset : IEntityChangeset<Floor, FloorId>
    {
        public Floor? Origin { get; }

        public Floor Apply()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetErrors()
        {
            throw new NotImplementedException();
        }

        public Floor ToEntity()
        {
            throw new NotImplementedException();
        }
    }
}
