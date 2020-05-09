using System;
using System.Collections.Generic;
using System.Text;

namespace TableTopCrucible.Domain.ValueTypes
{
    public struct Point3D
    {
        public Point3D(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        double X { get; }
        double Y { get; }
        double Z { get; }
    }
}
