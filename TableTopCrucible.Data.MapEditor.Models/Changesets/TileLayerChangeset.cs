using System;
using System.Collections.Generic;

using TableTopCrucible.Core.Models.ValueTypes;
using TableTopCrucible.Data.MapEditor.Models.IDs;

namespace TableTopCrucible.Data.MapEditor.Models.Changesets
{
    public class TileLayerChangeset : IEntityChangeset<TileLayer, TileLayerId>
    {
        public TileLayer? Origin { get; }

        public TileLayer Apply()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetErrors()
        {
            throw new NotImplementedException();
        }

        public TileLayer ToEntity()
        {
            throw new NotImplementedException();
        }
    }
}
