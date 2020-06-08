using System;
using System.Collections.Generic;
using System.Text;

namespace TableTopCrucible.Domain.Models.ValueTypes
{
    public struct Version
    {
        public Version(int major, int minor, int patch)
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
        }

        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }
    }
}
