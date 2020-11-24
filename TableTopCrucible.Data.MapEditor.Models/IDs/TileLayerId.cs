using System;

using TableTopCrucible.Core.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.MapEditor.Models.IDs
{
    public struct TileLayerId : ITypedId
    {
        private Guid _guid;
        public TileLayerId(Guid guid)
            => _guid = guid;
        public override bool Equals(object obj)
        {
            if (obj is TileLayerId id)
                return _guid == id._guid;
            return false;
        }

        public override int GetHashCode() => _guid.GetHashCode();
        public Guid ToGuid() => _guid;
        public static TileLayerId New() => (TileLayerId)Guid.NewGuid();
        public override string ToString() => _guid.ToString();
        public static bool operator ==(TileLayerId id1, TileLayerId id2)
            => id1._guid == id2._guid;
        public static bool operator !=(TileLayerId id1, TileLayerId id2)
            => id1._guid != id2._guid;

        public static explicit operator Guid(TileLayerId id)
            => id._guid;
        public static explicit operator TileLayerId(Guid id)
            => new TileLayerId(id);
    }
}
