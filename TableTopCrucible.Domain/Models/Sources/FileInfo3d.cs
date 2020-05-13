using System;
using System.Collections.Generic;

using TableTopCrucible.Domain.ValueTypes;
using TableTopCrucible.Domain.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public struct FileInfo3d
    {
        public FileInfo3d(IEnumerable<FilePath3d> paths, Point3D locationOffset, Orientation3D orientationOffset, string name) : this()
        {
            this.Paths = paths ?? throw new ArgumentNullException(nameof(paths));
            this.LocationOffset = locationOffset;
            this.OrientationOffset = orientationOffset;
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public FileInfoId Id { get; }
        public IEnumerable<FilePath3d> Paths { get; }
        public Point3D LocationOffset { get; }
        public Orientation3D OrientationOffset { get; }
        public string Name { get; }
        public Hash Hash { get; }
    }
}
