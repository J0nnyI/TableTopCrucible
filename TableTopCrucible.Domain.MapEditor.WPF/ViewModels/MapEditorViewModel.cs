using HelixToolkit.Wpf;

using MaterialDesignThemes.Wpf;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using System.Windows.Input;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.WPF.Helper;
using TableTopCrucible.Core.WPF.PageViewModels;
using TableTopCrucible.Data.MapEditor.Stores;
using TableTopCrucible.Domain.MapEditor.Core.Layers;
using TableTopCrucible.Domain.MapEditor.Core.Managers;
using TableTopCrucible.Domain.MapEditor.Core.Models;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.MapEditor.WPF.ViewModels
{
    public interface IMapEditorVm
    {
        IObservable<ItemId> SelectedItemIdChanges { get; set; }
    }
    public class MapEditorViewModel : DisposableReactiveObjectBase, IMapEditorVm
    {
        private readonly IMapDataService _mapDataService;
        private readonly IMapManager _mapManager;
        private readonly ISelectionManager _selectionManager;

        public HelixViewport3D Viewport { get; } = new HelixViewport3D();
        private Subject<RotationDirection> CursorRotationDeltaChanges { get; } = new Subject<RotationDirection>();
        [Reactive]
        public IObservable<ItemId> SelectedItemIdChanges { get; set; }

        public MapEditorViewModel(IMapDataService mapDataService, IMapManager mapManager,
            ISelectionManager selectionManager)
        {
            _mapDataService = mapDataService;
            _mapManager = mapManager;
            _selectionManager = selectionManager;
            Viewport.Children.Add(new DefaultLights(), mapManager.MasterModel);
            Viewport.ZoomGesture = new MouseGesture()
            {
                Modifiers = ModifierKeys.Control,
                MouseAction = MouseAction.MiddleClick
            };
            Viewport.PreviewMouseWheel += _viewport_PreviewMouseWheel;

            var map = this._mapManager.CreateMap();

            mapManager.SelectedItemIdChanges = this.WhenAnyObservable(vm => vm.SelectedItemIdChanges);
            mapManager.OnModelRotation = CursorRotationDeltaChanges;

            this.Viewport.KeyUp += _viewport_KeyUp;
        }

        private void _viewport_KeyUp(object sender, KeyEventArgs e)=> this._selectionManager.OnViewportKeyUp(e);

        private void _viewport_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (KeyboardHelper.IsKeyPressed(ModifierKeys.Control))
                return;
            e.Handled = true;
            this.CursorRotationDeltaChanges.OnNext(e.Delta > 0 ? RotationDirection.Right : RotationDirection.Left);
        }
    }
}
