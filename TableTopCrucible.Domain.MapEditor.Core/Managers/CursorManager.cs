using HelixToolkit.Wpf;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Domain.MapEditor.Core.Models;
using TableTopCrucible.Domain.MapEditor.Core.Services;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.MapEditor.Core.Managers
{
    public interface ICursorManager
    {
        Visual3D MasterModel { get; }
        public IObservable<RotationDirection> OnModelRotation { get; set; }
        double CurrentRotation { get; }
    }
    public class CursorManager : DisposableReactiveObjectBase, ICursorManager
    {
        private readonly ModelUIElement3D _masterModel = new ModelUIElement3D() { IsHitTestVisible = false };
        private readonly Model3DGroup _rotationModel = new Model3DGroup();
        private readonly ISelectionManager _selectionManager;

        public Visual3D MasterModel => _masterModel;
        [Reactive]
        public IObservable<RotationDirection> OnModelRotation { get; set; }
        [Reactive]
        public double CurrentRotation { get; private set; } = 0;

        public CursorManager(IModelCache modelCache, ISelectionManager selectionManager)
        {
            var idChanges =selectionManager.WhenAnyValue(vm=>vm.SelectedItem).Select(item=>item.ItemId);
            this._selectionManager = selectionManager;

            _masterModel.Model = _rotationModel;

            modelCache.Get(idChanges)
                .Subscribe(x => { });

            var mat = new DiffuseMaterial(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA7BEFB")));
            modelCache.Get(idChanges)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(model=> {
                    var res = model?.Clone();
                    res?.SetMaterial(mat);
                    return res;
                })
                .CombineLatest(
                    selectionManager
                        .OnFieldMouseEnter
                        .DistinctUntilChanged()
                        .Do(x=> { }),
                     this._selectionManager
                        .WhenAnyValue(vm => vm.ShowTileCursor),
                    (model, mouseEnterArgs, showCursor) => { return new { model, mouseEnterArgs, showCursor }; }
                )
                .Subscribe(x =>
                {
                    if (!x.showCursor) { 
                        _rotationModel.Children.Clear();
                        return;
                    }
                    if (!_rotationModel.Children.Contains(x.model))
                    {
                        _rotationModel.Children.Clear();
                        if (x?.model != null)
                            _rotationModel.Children.Add(x.model);
                    }

                    moveModel(x.mouseEnterArgs.Location);
                });

            this.WhenAnyObservable(vm => vm.OnModelRotation)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeOn(RxApp.MainThreadScheduler)
                .Subscribe(value =>
                {
                    rotateModel(value);
                });

            this.WhenAnyValue(vm => vm.CurrentRotation)
                .BindTo(this._selectionManager, vm => vm.CursorRotation);
        }
        private void moveModel(Rect3D location)
        {
            var matrix = _masterModel.Transform.Value;

            matrix.OffsetX = location.X + location.SizeX / 2;
            matrix.OffsetY = location.Y + location.SizeY / 2;
            matrix.OffsetZ = location.Z;

            _masterModel.Transform = new MatrixTransform3D(matrix);
        }
        private void rotateModel(RotationDirection direction)
        {
            var angle = 90;

            if (direction == RotationDirection.Left)
                angle*=-1;

            CurrentRotation += angle;

            if (CurrentRotation == 360)
                CurrentRotation = 0;
            if (CurrentRotation == -90)
                CurrentRotation = 360 - 90;

            var matrix = _rotationModel.Transform.Value;
            matrix.Rotate(new Quaternion(new Vector3D(0, 0, 1), angle));
            _rotationModel.Transform = new MatrixTransform3D(matrix);
        }
    }
}
