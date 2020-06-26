using System;

using TableTopCrucible.Core.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.ValueTypes.IDs
{
    public struct TileTypeId : ITypedId
    {
        private Guid _guid;
        public TileTypeId(Guid guid)
            => this._guid = guid;

        public override bool Equals(object obj)
        {
            if (obj is TileTypeId id)
                return this._guid == id._guid;
            return false;
        }

        public override int GetHashCode()
            => this._guid.GetHashCode();
        public Guid ToGuid()
            => this._guid;

        public static bool operator ==(TileTypeId id1, TileTypeId id2)
            => id1._guid == id2._guid;
        public static bool operator !=(TileTypeId id1, TileTypeId id2)
            => id1._guid != id2._guid;
        public static explicit operator Guid(TileTypeId id)
            => id._guid;
        public static explicit operator TileTypeId(Guid id)
            => new TileTypeId(id);
    }
}
