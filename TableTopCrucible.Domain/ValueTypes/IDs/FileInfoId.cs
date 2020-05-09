using System;

namespace TableTopCrucible.Domain.ValueTypes.IDs
{
    public struct FileInfoId:ITypedId
    {
        private Guid _guid;
        public FileInfoId(Guid guid)
            => this._guid = guid;
        
        public override bool Equals(object obj)
        {
            if (obj is FileInfoId id)
                return this._guid == id._guid;
            return false;
        }

        public override int GetHashCode() => this._guid.GetHashCode();
        public Guid ToGuid()
            => this._guid; 

        public static bool operator ==(FileInfoId id1, FileInfoId id2)
            => id1._guid == id2._guid;
        public static bool operator !=(FileInfoId id1, FileInfoId id2)
            => id1._guid != id2._guid;
        public static  explicit operator Guid (FileInfoId id)
            =>id._guid;
        public static  explicit operator FileInfoId (Guid id)
            =>new FileInfoId(id);
    }
}
