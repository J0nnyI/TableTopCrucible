using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.WPF.Helper;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Data.MapEditor.Models.Sources;
using TableTopCrucible.Data.MapEditor.Stores;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Domain.MapEditor.Core.Layers;
using TableTopCrucible.Domain.MapEditor.Core.Models;
using TableTopCrucible.Domain.MapEditor.Core.Models.Views;

namespace TableTopCrucible.Domain.MapEditor.Core.Managers
{
    public interface ISelectionManager
    {
        FloorId SelectedFloor { get; set; }
        IObservable<GridLayerMouseArgs> OnFieldMouseEnter { get; }
        ItemEx SelectedItem { get; set; }
        double CursorRotation { get; set; }
        bool ShowTileCursor { get; }

        void TileMouseEnter(TileManager tileManager);
        void TileMouseDown(TileManager tileManager, MouseButtonEventArgs args);
        void FieldMouseEnter(GridLayerMouseArgs args);
        void FieldMouseDown(Rect3D fullField, MouseButtonEventArgs args);
        void OnViewportKeyUp(KeyEventArgs e);
    }
    public class SelectionManager : DisposableReactiveObjectBase, ISelectionManager
    {

        public SelectionManager(ITileLocationDataService tileLocationDataService)
        {
            Observable.Merge(
                    _onTileMouseEnter.Select(_ => false),
                    _onFieldMouseEnter.Select(_ => true)
                )
                .DistinctUntilChanged()
                .BindTo(this, vm => vm.ShowTileCursor);


            SelectedTiles
                .Connect()
                .OnItemAdded(tile => tile.IsSelected = true)
                .OnItemRemoved(tile => tile.IsSelected = false)
                .Subscribe();

            Observable.Merge(this._onFieldMouseDown, this.OnFieldMouseEnter)
                .Where(x => x.MouseEventArgs.LeftButton == MouseButtonState.Pressed)
                .WithLatestFrom(this.WhenAnyValue(vm => vm.SelectedFloor), (location, floorId) => { return new { location, floorId }; })
                .WithLatestFrom(this.WhenAnyValue(vm => vm.SelectedItem), (x, item) => { return new { x.location, x.floorId, item }; })
                .WithLatestFrom(this.WhenAnyValue(vm => vm.CursorRotation), (x, rotation) => { return new { x.location, x.floorId, x.item, rotation }; })
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Subscribe(x =>
                {
                    var location = new TileLocation(
                        x.item.ItemId,
                        x.floorId,
                        new Point(Convert.ToInt32(x.location.Location.X + x.location.Location.SizeX / 2), Convert.ToInt32(x.location.Location.Y + x.location.Location.SizeY / 2)),
                        x.rotation);
                    tileLocationDataService.Post(location);
                });
            _tileLocationDataService = tileLocationDataService;
        }
        [Reactive]
        public double CursorRotation { get; set; }
        [Reactive]
        public bool ShowTileCursor { get; private set; } = true;
        [Reactive]
        public ItemEx SelectedItem { get; set; }
        [Reactive]
        public FloorId SelectedFloor { get; set; }
        public SourceList<TileManager> SelectedTiles { get; } = new SourceList<TileManager>();
        [Reactive]
        public TileManager HoveredTile { get; private set; }

        private Subject<TileManager> _onTileMouseEnter = new Subject<TileManager>();
        private Subject<GridLayerMouseArgs> _onFieldMouseEnter = new Subject<GridLayerMouseArgs>();
        private Subject<GridLayerMouseArgs> _onFieldMouseDown = new Subject<GridLayerMouseArgs>();
        private readonly ITileLocationDataService _tileLocationDataService;

        public IObservable<GridLayerMouseArgs> OnFieldMouseEnter => _onFieldMouseEnter;

        public void FieldMouseEnter(GridLayerMouseArgs args) => _onFieldMouseEnter.OnNext(args);
        public void FieldMouseDown(Rect3D fullField, MouseButtonEventArgs args) => _onFieldMouseDown.OnNext(new GridLayerMouseArgs(fullField, args));
        public void TileMouseEnter(TileManager tileManager) => _onTileMouseEnter.OnNext(tileManager);
        public void TileMouseDown(TileManager tileManager, MouseButtonEventArgs args)
        {
            if (args.LeftButton == MouseButtonState.Pressed)
            {
                Observable.Start(() =>
                {
                    if (!KeyboardHelper.IsKeyPressed(ModifierKeys.Control))
                        SelectedTiles.Clear();
                    if (tileManager.IsSelected)
                    {
                        SelectedTiles.Remove(tileManager);
                    }
                    else
                    {
                        SelectedTiles.Add(tileManager);
                        tileManager
                            .Destroy
                            .Subscribe(_ => SelectedTiles.Remove(tileManager));
                    }
                }, RxApp.TaskpoolScheduler);
            }
        }
        public void OnViewportKeyUp(KeyEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Delete:
                    this._tileLocationDataService.Delete(
                        this.SelectedTiles.Items.Select(tile => tile.LocationId));
                    this.SelectedTiles.Clear();
                    break;
            }

        }
    }
}
