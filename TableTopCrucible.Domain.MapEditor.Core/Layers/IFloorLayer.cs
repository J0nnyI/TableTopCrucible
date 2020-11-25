using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;

namespace TableTopCrucible.Domain.MapEditor.Core.Layers
{
    public interface IFloorLayer
    {
        public Visual3D Model { get; }
    }
}
