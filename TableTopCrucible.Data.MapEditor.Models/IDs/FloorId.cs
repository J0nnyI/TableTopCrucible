using System;

using TableTopCrucible.Core.Models.ValueTypes.IDs;

namespace TableTopCrucible.Data.MapEditor.Models.IDs
{
    public struct FloorId : ITypedId
    {
        private Guid _guid;
        public FloorId(Guid guid)
            => _guid = guid;
        public override bool Equals(object obj)
        {
            if (obj is FloorId id)
                return _guid == id._guid;
            return false;
        }

        public override int GetHashCode() => _guid.GetHashCode();
        public Guid ToGuid() => _guid;
        public static FloorId New() => (FloorId)Guid.NewGuid();
        public override string ToString() => _guid.ToString();
        public static bool operator ==(FloorId id1, FloorId id2)
            => id1._guid == id2._guid;
        public static bool operator !=(FloorId id1, FloorId id2)
            => id1._guid != id2._guid;

        public static explicit operator Guid(FloorId id)
            => id._guid;
        public static explicit operator FloorId(Guid id)
            => new FloorId(id);
    }
}
