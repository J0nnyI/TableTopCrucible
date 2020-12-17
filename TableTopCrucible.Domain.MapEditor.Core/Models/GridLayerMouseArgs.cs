
using System;
using System.Windows.Input;
using System.Windows.Media.Media3D;

using TableTopCrucible.Domain.MapEditor.Core.Managers;
namespace TableTopCrucible.Domain.MapEditor.Core.Models
{
    public struct GridLayerMouseArgs
    {
        public GridLayerMouseArgs(Rect3D location, MouseEventArgs mouseEventArgs)
        {
            Location = location;
            MouseEventArgs = mouseEventArgs ?? throw new ArgumentNullException(nameof(mouseEventArgs));
        }

        public Rect3D Location { get; }
        public MouseEventArgs MouseEventArgs { get; }
    }
}
