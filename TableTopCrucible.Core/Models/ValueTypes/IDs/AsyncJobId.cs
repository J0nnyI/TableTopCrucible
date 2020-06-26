using System;

using TableTopCrucible.Core.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.ValueTypes.IDs
{
    public struct AsyncJobId : ITypedId
    {
        private Guid _guid;
        public AsyncJobId(Guid guid)
            => this._guid = guid;
        public override bool Equals(object obj)
        {
            if (obj is AsyncJobId id)
                return this._guid == id._guid;
            return false;
        }

        public override int GetHashCode() => this._guid.GetHashCode();
        public Guid ToGuid() => this._guid;

        public override string ToString() => _guid.ToString();
        public static bool operator ==(AsyncJobId id1, AsyncJobId id2)
            => id1._guid == id2._guid;
        public static bool operator !=(AsyncJobId id1, AsyncJobId id2)
            => id1._guid != id2._guid;

        public static explicit operator Guid(AsyncJobId id)
            => id._guid;
        public static explicit operator AsyncJobId(Guid id)
            => new AsyncJobId(id);

        public static AsyncJobId New()
            => new AsyncJobId(Guid.NewGuid());
    }
}
