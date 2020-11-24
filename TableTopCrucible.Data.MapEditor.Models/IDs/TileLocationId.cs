using System;

using TableTopCrucible.Core.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.MapEditor.Models.IDs
{
    public struct TileLocationId : ITypedId
    {
        private Guid _guid;
        public TileLocationId(Guid guid)
            => _guid = guid;
        public override bool Equals(object obj)
        {
            if (obj is TileLocationId id)
                return _guid == id._guid;
            return false;
        }

        public override int GetHashCode() => _guid.GetHashCode();
        public Guid ToGuid() => _guid;
        public static TileLocationId New() => (TileLocationId)Guid.NewGuid();
        public override string ToString() => _guid.ToString();
        public static bool operator ==(TileLocationId id1, TileLocationId id2)
            => id1._guid == id2._guid;
        public static bool operator !=(TileLocationId id1, TileLocationId id2)
            => id1._guid != id2._guid;

        public static explicit operator Guid(TileLocationId id)
            => id._guid;
        public static explicit operator TileLocationId(Guid id)
            => new TileLocationId(id);
    }
}
