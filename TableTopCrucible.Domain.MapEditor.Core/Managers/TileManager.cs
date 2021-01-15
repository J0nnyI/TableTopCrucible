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
using TableTopCrucible.Domain.MapEditor.Core.Managers;
using TableTopCrucible.Domain.MapEditor.Core.Models;
using TableTopCrucible.Domain.MapEditor.Core.Models.Views;
namespace TableTopCrucible.Domain.MapEditor.Core.Models
{
}
namespace TableTopCrucible.Domain.MapEditor.Core.Managers
{
    public class TileManager : DisposableReactiveObjectBase
    {
        public TileManager(
            Model3DGroup model, 
            TileLocationEx location, 
            ContainerUIElement3D parent
            ,ISelectionManager selectionManager)
        {
            this._uiElement = new ModelUIElement3D { Model = model };
            this.LocationId = location.LocationId;

            var matrix = this._uiElement.Transform.Value;
            matrix.Rotate(new Quaternion(new Vector3D(0, 0, 1), location.HorizontalRotation));
            this._uiElement.Transform = new MatrixTransform3D(matrix);
            this._uiElement.Move(location.Origin);
            this._uiElement.IsHitTestVisible = true;

            parent.Children.Add(this._uiElement);


            this.Destroy
                .Take(1)
                .Subscribe(_ =>
                {
                    parent.Children.Remove(this._uiElement);
                }
            );
            var selectionMat = new DiffuseMaterial(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA7BEFB")));
            var selectionHoverMat = new DiffuseMaterial(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFc4d4ff")));
            var hoverMat = new DiffuseMaterial(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e8eeff")));
            this.WhenAnyValue(vm => vm.IsSelected).ObserveOn(RxApp.TaskpoolScheduler).Subscribe(isSelected =>
             {
                 if (IsSelected)
                 {
                     var newModel = model.Clone();
                     newModel.SetMaterial(selectionMat);
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
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Subscribe(x =>
                {
                    if (x.isSelected)
                    {
                        if (x.mouseOver)
                        {

                            var newModel = model.Clone();
                            newModel.SetMaterial(selectionMat);
                            this._uiElement.Model = newModel;
                        }
                        else
                        {
                            var newModel = model.Clone();
                            newModel.SetMaterial(selectionHoverMat);
                            this._uiElement.Model = newModel;
                        }
                        return;
                    }
                    if (x.mouseOver)
                    {
                        var clone = model.Clone();
                        clone.SetMaterial(hoverMat);
                        this._uiElement.Model = clone;
                        return;
                    }
                    this._uiElement.Model = model;

                });

            Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                action => this._uiElement.MouseEnter += action,
                action => this._uiElement.MouseEnter -= action)
                .Subscribe(_ => selectionManager.TileMouseEnter(this));

            Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>
                (action => this._uiElement.MouseDown += action, action => this._uiElement.MouseDown -= action)
                .Subscribe(x => selectionManager.TileMouseDown(this, x.EventArgs));
        }
        private ModelUIElement3D _uiElement { get; }
        public TileLocationId LocationId { get; }
        [Reactive]
        public Point Location { get; set; }
        [Reactive]
        public double HorizontalRotation { get; set; }
        [Reactive]
        public bool IsSelected { get; set; }
    }
}
