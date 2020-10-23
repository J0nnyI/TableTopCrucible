using Microsoft.VisualBasic.CompilerServices;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TableTopCrucible.Core.Models.ValueTypes
{
    [Serializable]
    public struct AbsolutePath
    {
        private readonly string path;

        public AbsolutePath(string path)
        {
            this.path = path;
        }
        public override string ToString() => path;

        public override bool Equals(object obj)
        {
            return obj is AbsolutePath path &&
                   this.path == path.path;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(path);
        }

        public static explicit operator string(AbsolutePath path) => path.path;
        public static explicit operator AbsolutePath(string path) => new AbsolutePath(path);
        public static bool operator ==(AbsolutePath p1, AbsolutePath p2) => p1.path == p2.path;
        public static bool operator !=(AbsolutePath p1, AbsolutePath p2) => p1.path != p2.path;

    }
}
