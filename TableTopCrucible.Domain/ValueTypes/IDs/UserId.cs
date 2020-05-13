using System;

namespace TableTopCrucible.Domain.ValueTypes.IDs
{
    public struct UserId : ITypedId
    {
        private Guid _guid;
        public UserId(Guid guid)
            => this._guid = guid;

        public override bool Equals(object obj)
        {
            if (obj is UserId id)
                return this._guid == id._guid;
            return false;
        }

        public override int GetHashCode()
            => this._guid.GetHashCode();
        public Guid ToGuid()
            => this._guid;

        public static bool operator ==(UserId id1, UserId id2)
            => id1._guid == id2._guid;
        public static bool operator !=(UserId id1, UserId id2)
            => id1._guid != id2._guid;
        public static explicit operator Guid(UserId id)
            => id._guid;
        public static explicit operator UserId(Guid id)
            => new UserId(id);
    }
}
