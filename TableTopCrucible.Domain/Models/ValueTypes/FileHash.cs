using System;
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
        public override string ToString() => BitConverter.ToString(this.Data);
    }
}
