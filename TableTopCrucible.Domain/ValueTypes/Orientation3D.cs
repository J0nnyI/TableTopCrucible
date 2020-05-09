using System;
using System.Collections.Generic;
using System.Text;

namespace TableTopCrucible.Domain.ValueTypes
{
    public struct Orientation3D
    {
        public double Pitch { get; }
        public double Yaw{ get; }
        public double Roll{ get; }
    }
}
