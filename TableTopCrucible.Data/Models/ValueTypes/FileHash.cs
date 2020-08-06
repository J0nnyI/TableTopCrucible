using System;
using System.Collections.Generic;
using System.Linq;

namespace TableTopCrucible.Domain.Models.ValueTypes
{
    public struct FileHash
    {
        public FileHash(byte[] data)
        {
            this.Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public byte[] Data { get; }

        public override bool Equals(object obj)
            => obj is FileHash hash &&
            (this.Data != null && hash.Data != null && Enumerable.SequenceEqual(this.Data, hash.Data) ||
            (this.Data == null && hash.Data == null));
        public override int GetHashCode()
            => this.ToString().GetHashCode();
        public override string ToString()
            => BitConverter.ToString(this.Data);

        public static bool operator !=(FileHash hashA, FileHash hashB)
            => !hashA.Equals(hashB);
        public static bool operator ==(FileHash hashA, FileHash hashB)
            => hashA.Equals(hashB);
    }
}
