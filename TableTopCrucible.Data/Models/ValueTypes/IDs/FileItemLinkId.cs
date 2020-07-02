using System;

using TableTopCrucible.Core.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.ValueTypes.IDs
{
    public struct FileItemLinkId : ITypedId
    {
        private Guid _guid;
        public FileItemLinkId(Guid guid)
            => this._guid = guid;
        public override bool Equals(object obj)
        {
            if (obj is FileItemLinkId id)
                return this._guid == id._guid;
            return false;
        }

        public override int GetHashCode() => this._guid.GetHashCode();
        public Guid ToGuid() => this._guid;

        public override string ToString() => _guid.ToString();
        public static bool operator ==(FileItemLinkId id1, FileItemLinkId id2)
            => id1._guid == id2._guid;
        public static bool operator !=(FileItemLinkId id1, FileItemLinkId id2)
            => id1._guid != id2._guid;

        public static explicit operator Guid(FileItemLinkId id)
            => id._guid;
        public static explicit operator FileItemLinkId(Guid id)
            => new FileItemLinkId(id);
    }
}
