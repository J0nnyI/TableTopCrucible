using System;
using System.Collections.Generic;
using System.Windows.Media;

using TableTopCrucible.Domain.Models;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.ValueTypes.IDs;

namespace TableTopCrucible.Domain.ValueTypes
{
    public interface ITileTypeChangeset : IEntityChangeset<TileType, TileTypeId> { }

    public class TileTypeChangeset : EntityChangesetBase<TileType, TileTypeId>, ITileTypeChangeset
    {
        public TileTypeChangeset(TileType? origin) : base(origin)
        {
        }

        public override TileType Apply() => throw new NotImplementedException();
        public override IEnumerable<string> GetErrors() => throw new NotImplementedException();
        public override TileType ToEntity() => throw new NotImplementedException();
    }
    public struct TileType : IEntity<TileTypeId>
    {

        public TileTypeId Id { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }

        public int Width { get; }
        public int Height { get; }
        private Color[,] colorGrid;
        public Color this[int x, int y] { get => colorGrid[x, y]; }
    }
}
