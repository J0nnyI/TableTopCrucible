using System;

using TableTopCrucible.Core.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.ValueTypes.IDs
{
    public struct DirectorySetupId : ITypedId
    {
        private Guid _guid;
        public DirectorySetupId(Guid guid)
            => this._guid = guid;
        public override bool Equals(object obj)
        {
            if (obj is DirectorySetupId id)
                return this._guid == id._guid;
            return false;
        }

        public override int GetHashCode() => this._guid.GetHashCode();
        public Guid ToGuid() => this._guid;

        public static DirectorySetupId New()
            => (DirectorySetupId)Guid.NewGuid();

        public override string ToString() => _guid.ToString();
        public static bool operator ==(DirectorySetupId id1, DirectorySetupId id2)
            => id1._guid == id2._guid;
        public static bool operator !=(DirectorySetupId id1, DirectorySetupId id2)
            => id1._guid != id2._guid;

        public static explicit operator Guid(DirectorySetupId id)
            => id._guid;
        public static explicit operator DirectorySetupId(Guid id)
            => new DirectorySetupId(id);
    }
}
