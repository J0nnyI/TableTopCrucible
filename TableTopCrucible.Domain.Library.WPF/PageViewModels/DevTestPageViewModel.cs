﻿using DynamicData;

using HelixToolkit.Wpf;

using MaterialDesignThemes.Wpf;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Media3D;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.WPF.PageViewModels;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Data.MapEditor.Models.Sources;
using TableTopCrucible.Data.MapEditor.Stores;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Library.WPF.Commands;
using TableTopCrucible.Domain.Library.WPF.ViewModels;
using TableTopCrucible.Domain.MapEditor.Core.Layers;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.PageViewModels
{
    public class DevTestPageViewModel : PageViewModelBase
    {
        [Reactive]
        public HelixViewport3D Viewport { get; set; }
        public ProjectionCamera Camera { get; } = new OrthographicCamera();
        private readonly IFloorDataService floorDataService;

        public ICommand ThrowExCmd { get; } = new RelayCommand(_ => throw new Exception("crash it baby"));
        private ModelUIElement3D modelCursor;
        private Model3DGroup model;
        public DevTestPageViewModel(
            CreateThumbnailsCommand createAllThumbnails,
            NotificationCenterViewModel notificationCenterViewModel,
            CreateThumbnailsCommand generateThumbnails,
            IGridLayer tileGrid,
            IFloorDataService floorDataService
            ) : base("dev test", PackIconKind.DeveloperBoard)
        {
            CreateAllThumbnails = createAllThumbnails;
            NotificationCenterViewModel = notificationCenterViewModel;
            GenerateThumbnails = generateThumbnails;
            TileGrid = tileGrid;
            this.floorDataService = floorDataService;

            var mapId = MapId.New();
            var floor = new Floor(null, mapId, 0);
            floorDataService.Post(floor);
            tileGrid.GridSize = 51;
            tileGrid.FloorId = floor.Id;
            this.model = importModel();
            this.modelCursor = new ModelUIElement3D
            {
                Model = model,
                IsHitTestVisible = false
            };


            this.ObservableForProperty(vm => vm.Viewport)
                .Take(1)
                .Select(x => x.Value)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(vp => vp != null)
                .Subscribe(vp =>
                {
                    vp.Children.AddRange(new Visual3D[]{
                        tileGrid.Model,
                        modelCursor
                    });
                    vp.Camera = Camera;
                });

            TileGrid
                .FieldMouseEnter
                .TakeUntil(destroy)
                .Subscribe(location =>
                {
                    moveToCoordinate(modelCursor, location);
                });

            TileGrid
                .FieldSelected
                .TakeUntil(destroy)
                .Subscribe(location =>
                {
                    var visual = new ModelVisual3D
                    {
                        Content = this.model
                    };
                    moveToCoordinate(visual, location);
                    Viewport.Children.Add(visual);
                });
        }
        private void moveToCoordinate(Visual3D model, Rect3D location)
        {
            var matrix = model.Transform.Value;

            matrix.OffsetX = location.X + location.SizeX / 2;
            matrix.OffsetY = location.Y + location.SizeY / 2;
            matrix.OffsetZ = location.Z;

            model.Transform = new MatrixTransform3D(matrix);
        }
        private Model3DGroup importModel()
        {
            ModelImporter importer = new ModelImporter()
            {
                DefaultMaterial = Materials.Green
            };
            //var model = importer.Load(@"D:\3d Demofiles\FDG0280_Floor_Wood_2x2.stl");
            var model = importer.Load(@"D:\3d Demofiles\FDG0280_Floor_Wood_4x2.stl");
            model.PlaceAtOrigin();
            return model;

        }


        public CreateThumbnailsCommand CreateAllThumbnails { get; }
        public NotificationCenterViewModel NotificationCenterViewModel { get; }
        public CreateThumbnailsCommand GenerateThumbnails { get; }
        public IGridLayer TileGrid { get; }
    }
}
