using System;

using TableTopCrucible.Data.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.ValueTypes
{
    public struct FileInfoHashKey
    {
        public FileHash FileHash { get; }
        public long FileSize { get; }
        public FileInfoHashKey(FileInfo fileInfo)
        {
            this.FileHash = fileInfo.FileHash.Value;
            this.FileSize = fileInfo.FileSize;
        }
        /// <summary>
        /// shoould be only used when there is not fileInfo
        /// </summary>
        /// <param name="fileHash"></param>
        /// <param name="fileSize"></param>
        public FileInfoHashKey(FileHash fileHash, long fileSize)
        {
            this.FileHash = fileHash;
            this.FileSize = fileSize;
        }

        public static bool CanBuild(FileInfo fileInfo)
            => fileInfo.FileHash.HasValue;

        public override string ToString() => $"{FileHash} : ${FileSize}";
        public override bool Equals(object obj)
        => obj is FileInfoHashKey key && this.FileHash == key.FileHash && this.FileSize == key.FileSize;
        public override int GetHashCode() => this == default ? 0 : HashCode.Combine(this.FileHash, this.FileSize);

        public static bool operator ==(FileInfoHashKey key1, FileInfoHashKey key2)
            => key1.Equals(key2);
        public static bool operator !=(FileInfoHashKey key1, FileInfoHashKey key2)
            => !key1.Equals(key2);
    }

}
