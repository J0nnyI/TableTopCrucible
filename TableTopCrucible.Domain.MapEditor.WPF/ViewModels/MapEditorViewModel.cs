using HelixToolkit.Wpf;

using MaterialDesignThemes.Wpf;

using System;
using System.Collections.Generic;
using System.Text;
using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.WPF.PageViewModels;
using TableTopCrucible.Domain.MapEditor.Core;
using TableTopCrucible.Domain.MapEditor.Core.Layers;

namespace TableTopCrucible.Domain.MapEditor.WPF.ViewModels
{
    public interface IMapEditorVm
    {

    }
    public class MapEditorViewModel : DisposableReactiveObjectBase, IMapEditorVm
    {
        private readonly IFloorManager floorManager;

        public HelixViewport3D Viewport { get; } = new HelixViewport3D();
        public MapEditorViewModel(IFloorManager floorManager)
        {
            this.floorManager = floorManager;

            Viewport.Children.Add(new DefaultLights(), floorManager.MasterModel);
        }

    }
}
