using System;
using System.Diagnostics.CodeAnalysis;

namespace TableTopCrucible.Domain.Models.ValueTypes
{
    public struct Version : IComparable<Version>
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

        public int CompareTo([AllowNull] Version other)
        {
            if (other == default)
                return -1;

            if (this.Major < other.Major)
                return -1;
            if (this.Major > other.Major)
                return 1;

            if (this.Minor < other.Minor)
                return -1;
            if (this.Minor > other.Minor)
                return 1;

            if (this.Patch < other.Patch)
                return -1;
            if (this.Patch > other.Patch)
                return 1;

            return 0;
        }

        public override bool Equals(object obj) => obj is Version version && this.Major == version.Major && this.Minor == version.Minor && this.Patch == version.Patch;
        public override int GetHashCode() => HashCode.Combine(this.Major, this.Minor, this.Patch);
        public static bool operator ==(Version v1, Version v2)
        => v1.Equals(v2);
        public static bool operator !=(Version v1, Version v2)
        => !v1.Equals(v2);
    }
}
