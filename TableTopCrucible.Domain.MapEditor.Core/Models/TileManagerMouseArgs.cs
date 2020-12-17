
using System;
using System.Windows.Input;

using TableTopCrucible.Domain.MapEditor.Core.Managers;
namespace TableTopCrucible.Domain.MapEditor.Core.Models
{
    public struct TileManagerMouseArgs
    {
        public TileManagerMouseArgs(TileManager tile, MouseEventArgs mouseEventArgs)
        {
            TileManager = tile ?? throw new ArgumentNullException(nameof(tile));
            MouseEventArgs = mouseEventArgs ?? throw new ArgumentNullException(nameof(mouseEventArgs));
        }

        public TileManager TileManager { get; }
        public MouseEventArgs MouseEventArgs { get; }
    }
}
