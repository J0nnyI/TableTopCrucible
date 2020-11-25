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

namespace TableTopCrucible.Domain.MapEditor.Core
{
    public interface ITileGrid
    {
        ContainerUIElement3D Model { get; }
        FloorId FloorId { get; set; }
        double GridSize { get; set; }
        Subject<Rect3D> OnFieldHover { get; }
        Subject<Rect3D> OnFieldClicked { get; }
    }
    public class TileGrid : DisposableReactiveObjectBase, ITileGrid
    {
        private readonly IFloorDataService floorDataService;
        private readonly ITileLocationDataService tileLocationDataService;

        [Reactive]
        public FloorId FloorId { get; set; }
        [Reactive]
        public double GridSize { get; set; }

        public Subject<Rect3D> OnFieldHover { get; } = new Subject<Rect3D>();
        public Subject<Rect3D> OnFieldClicked { get; } = new Subject<Rect3D>();


        public ContainerUIElement3D Model { get; } = new ContainerUIElement3D();

        public TileGrid(IFloorDataService floorDataService, ITileLocationDataService tileLocationDataService)
        {
            this.floorDataService = floorDataService;
            this.tileLocationDataService = tileLocationDataService;





            var floorIdChanges = this.WhenAnyValue(vm => vm.FloorId);


            var size = this.tileLocationDataService.SizeByFloor(floorIdChanges);
            var floorChanges = floorIdChanges.Select(id => floorDataService.Get().WatchValue(id))
                .Switch();
            var gridSizeChanges = this.WhenAnyValue(vm => vm.GridSize);
            floorChanges.Subscribe(AlignmentX =>
            {

            });
            size.Subscribe(AlignmentX =>
            {

            });
            gridSizeChanges.Subscribe(x =>
            {

            });
            Observable
                .CombineLatest(
                    floorChanges,
                    size,
                    gridSizeChanges,
                    (floor, sizes, fieldSize) => { return new { floor, sizes, fieldSize }; })
                .TakeUntil(destroy)
                .Subscribe(x => handleSizeChange(x.floor, x.sizes, x.fieldSize));






            var currentFloor = floorIdChanges
                .Select(floor => floorDataService.Get().WatchValue(floor))
                .Switch();


        }
        private void handleSizeChange(Floor floor, Rect size, double fieldSize)
        {
            var border = fieldSize * 10;

            var elements = new List<ModelUIElement3D>();
            for (double x = size.Left - fieldSize / 2 - border; x < size.Right + border; x += fieldSize)
            {
                for (double y = size.Bottom - fieldSize / 2 - border; y < size.Top + border; y += fieldSize)
                {
                    var fullField = new Rect3D(x, y, floor.Height, fieldSize, fieldSize, floor.Height);

                    var uiElement = new ModelUIElement3D()
                    {
                        Model = new GeometryModel3D(_buildRect(x, y, fieldSize, floor.Height), Materials.LightGray)
                    };

                    uiElement.MouseEnter += (s, _) =>
                    {
                        if (s is ModelUIElement3D uie && uie.Model is GeometryModel3D geom)
                        {
                            geom.Material = Materials.Red;
                        }
                        this.OnFieldHover.OnNext(fullField);
                    };
                    uiElement.MouseLeave += (s, _) =>
                    {
                        if (s is ModelUIElement3D uie && uie.Model is GeometryModel3D geom)
                        {
                            geom.Material = Materials.Orange;
                        }
                    };
                    uiElement.MouseLeftButtonDown += (s, _) =>
                    {
                        if (s is ModelUIElement3D uie && uie.Model is GeometryModel3D geom)
                        {
                            geom.Material = Materials.Blue;
                        }
                        this.OnFieldClicked.OnNext(fullField);
                    };
                    elements.Add(uiElement);
                }
            }
            Model.Children.Clear();
            Model.Children.AddRange(elements);
        }
        private MeshGeometry3D _buildRect(double x, double y, double size, double height)
        {
            var builder = new MeshBuilder();
            builder.AddBox(new Rect3D(x + 2, y + 2, height - 2, size - 4, size - 4, 1)/*, BoxFaces.Top*/);
            return builder.ToMesh();
        }

    }
}
