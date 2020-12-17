using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Media;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Models;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Data.MapEditor.Models.Sources;
using TableTopCrucible.Data.MapEditor.Stores;
using System.Windows.Media;
using HelixToolkit.Wpf;
using System.Reactive.Subjects;
using System.Windows.Input;
using TableTopCrucible.Domain.MapEditor.Core.Managers;
using TableTopCrucible.Domain.MapEditor.Core.Models;

namespace TableTopCrucible.Domain.MapEditor.Core.Layers
{
    public interface IGridLayer
    {
        FloorId FloorId { get; set; }
        double GridSize { get; set; }
        Visual3D MasterModel { get; }
    }
    public class GridLayer : DisposableReactiveObjectBase, IGridLayer
    {
        private readonly ITileLocationDataService tileLocationDataService;
        private readonly ISelectionManager _selectionManager;

        [Reactive]
        public FloorId FloorId { get; set; }
        [Reactive]
        public double GridSize { get; set; } = 51;


        public Visual3D MasterModel => masterModel;
        private readonly ContainerUIElement3D masterModel = new ContainerUIElement3D();

        public GridLayer(
            IFloorDataService floorDataService,
            ITileLocationDataService tileLocationDataService,
            ISelectionManager selectionManager)
        {
            this.tileLocationDataService = tileLocationDataService;
            _selectionManager = selectionManager;
            var floorIdChanges = this.WhenAnyValue(vm => vm.FloorId);



            Observable.CombineLatest(
                floorIdChanges
                    .Select(id => floorDataService.Get().WatchValue(id))
                    .Switch(),
                this.tileLocationDataService
                    .SizeByFloor(floorIdChanges),
                this.WhenAnyValue(vm => vm.GridSize),
                (floor, floorSize, fieldSize) => { return new { floor, floorSize, fieldSize }; }
                )
                .TakeUntil(Destroy)
                .Subscribe(x => _handleSizeChange(x.floor, x.floorSize, x.fieldSize));

            var currentFloor = floorIdChanges
                .Select(floor => floorDataService.Get().WatchValue(floor))
                .Switch();


        }
        private void _handleSizeChange(Floor floor, Rect floorSize, double fieldSize)
        {
            var border = fieldSize * 10;

            var elements = new List<ModelUIElement3D>();

            var minX = floorSize.Left.Max(0) - (fieldSize / 2) - border;
            var maxX = floorSize.Right.Min(0) + border;
            var minY = floorSize.Top.Max(0) - (fieldSize / 2) - border;
            var maxY = floorSize.Bottom.Min(0) + border;


            for (double x = minX; x < maxX; x += fieldSize)
            {
                for (double y = minY; y < maxY; y += fieldSize)
                {
                    var fullField = new Rect3D(x, y, floor.Height, fieldSize, fieldSize, floor.Height);

                    var uiElement = new ModelUIElement3D()
                    {
                        Model = new GeometryModel3D(_buildRect(x, y, fieldSize, floor.Height), Materials.LightGray)
                    };

                    uiElement.MouseEnter += (s, args) =>
                        _selectionManager.FieldMouseEnter(new GridLayerMouseArgs(fullField, args));
                    uiElement.MouseDown += (s, args) =>
                        _selectionManager.FieldMouseDown(fullField, args);
                    elements.Add(uiElement);
                }
            }
            masterModel.Children.Clear();
            masterModel.Children.AddRange(elements);
        }
        private MeshGeometry3D _buildRect(double x, double y, double size, double height)
        {
            var builder = new MeshBuilder();
            builder.AddBox(new Rect3D(x + 2, y + 2, height - 2, size - 4, size - 4, 1)/*, BoxFaces.Top*/);
            return builder.ToMesh();
        }

    }
}
