using System;

using TableTopCrucible.Core.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.MapEditor.Models.IDs
{
    public struct MapId : ITypedId
    {
        private Guid _guid;
        public MapId(Guid guid)
            => _guid = guid;
        public override bool Equals(object obj)
        {
            if (obj is MapId id)
                return _guid == id._guid;
            return false;
        }

        public override int GetHashCode() => _guid.GetHashCode();
        public Guid ToGuid() => _guid;
        public static MapId New() => (MapId)Guid.NewGuid();
        public override string ToString() => _guid.ToString();
        public static bool operator ==(MapId id1, MapId id2)
            => id1._guid == id2._guid;
        public static bool operator !=(MapId id1, MapId id2)
            => id1._guid != id2._guid;

        public static explicit operator Guid(MapId id)
            => id._guid;
        public static explicit operator MapId(Guid id)
            => new MapId(id);
    }
}
