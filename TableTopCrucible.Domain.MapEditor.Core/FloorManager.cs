using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;
using TableTopCrucible.Domain.MapEditor.Core.Layers;

namespace TableTopCrucible.Domain.MapEditor.Core
{
    public interface IFloorManager
    {
        public Visual3D Model { get; }

    }
    public class FloorManager:IFloorManager
    {
        public ModelVisual3D model = new ModelVisual3D();
        public Visual3D Model => model;
        public IGridLayer GridLayer { get; }
        public FloorManager(IGridLayer gridLayer)
        {
            this.GridLayer = gridLayer;
        }
    }
}
