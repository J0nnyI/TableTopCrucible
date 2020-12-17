using HelixToolkit.Wpf;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Domain.MapEditor.Core.Models.Views;

namespace TableTopCrucible.Domain.MapEditor.Core.Managers
{
    public class TileManager : DisposableReactiveObjectBase
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
        public TileManager(Model3DGroup model, TileLocationEx location, ContainerUIElement3D parent)
        {
            this._uiElement = new ModelUIElement3D { Model = model };
            this.LocationId = location.LocationId;

            var matrix = this._uiElement.Transform.Value;
            matrix.Rotate(new Quaternion(new Vector3D(0, 0, 1), location.HorizontalRotation));
            this._uiElement.Transform = new MatrixTransform3D(matrix);
            this._uiElement.Move(location.Origin);
            this._uiElement.IsHitTestVisible = true;

            parent.Children.Add(this._uiElement);

            this.TileMouseDown = Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>
                (action => this._uiElement.MouseDown += action, action => this._uiElement.MouseDown -= action)
                .Select(args => new TileManagerMouseArgs(this, args.EventArgs))
                .TakeUntil(destroy);

            this.destroy
                .Take(1)
                .Subscribe(_ =>
                {
                    parent.Children.Remove(this._uiElement);
                }
            );
            var mat = new DiffuseMaterial(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA7BEFB")));
            this.WhenAnyValue(vm => vm.IsSelected).Subscribe(isSelected =>
             {
                 if (IsSelected)
                 {
                     var newModel = model.Clone();
                     newModel.SetMaterial(mat);
                     this._uiElement.Model = newModel;
                 }
                 else
                 {
                     this._uiElement.Model = model;
                 }
             });

            this.WhenAnyValue(
                vm => vm._uiElement.IsMouseDirectlyOver,
                vm => vm.IsSelected,
                (mouseOver, isSelected) => { return new { mouseOver, isSelected }; }
                )
                .Subscribe(x =>
                {
                    if (x.isSelected)
                    {
                        var newModel = model.Clone();
                        newModel.SetMaterial(mat);
                        this._uiElement.Model = newModel;
                        return;
                    }
                    if (x.mouseOver)
                    {
                        var clone = model.Clone();
                        clone.SetMaterial(Materials.White);
                        this._uiElement.Model = clone;
                        return;
                    }
                    this._uiElement.Model = model;

                });
        }
        private ModelUIElement3D _uiElement { get; }
        public TileLocationId LocationId { get; }
        public IObservable<TileManagerMouseArgs> TileMouseEnter { get; }
        public IObservable<TileManagerMouseArgs> TileMouseLeave { get; }
        public IObservable<TileManagerMouseArgs> TileMouseDown { get; }
        [Reactive]
        public Point Location { get; set; }
        [Reactive]
        public double HorizontalRotation { get; set; }
        [Reactive]
        public bool IsSelected { get; set; }
    }
}
