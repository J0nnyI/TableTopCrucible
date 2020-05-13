using System;

namespace TableTopCrucible.Domain.ValueTypes.IDs
{
    public struct CommentId : ITypedId
    {
        private Guid _guid;
        public CommentId(Guid guid)
            => this._guid = guid;
        public override bool Equals(object obj)
        {
            if (obj is CommentId id)
                return this._guid == id._guid;
            return false;
        }

        public override int GetHashCode() => this._guid.GetHashCode();
        public Guid ToGuid() => this._guid;

        public static bool operator ==(CommentId id1, CommentId id2)
            => id1._guid == id2._guid;
        public static bool operator !=(CommentId id1, CommentId id2)
            => id1._guid != id2._guid;

        public static explicit operator Guid(CommentId id)
            => id._guid;
        public static explicit operator CommentId(Guid id)
            => new CommentId(id);
    }
}
