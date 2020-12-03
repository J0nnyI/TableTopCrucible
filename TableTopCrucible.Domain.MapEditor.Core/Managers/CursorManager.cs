using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Media.Media3D;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Domain.MapEditor.Core.Services;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.MapEditor.Core.Managers
{
    public interface ICursorManager
    {
        Visual3D MasterModel { get; }
        IObservable<ItemId> SelectedItemIdChanges { get; set; }
        IObservable<Rect3D> LocationChanges { get; set; }
    }
    public class CursorManager : DisposableReactiveObjectBase, ICursorManager
    {
        private readonly ModelUIElement3D _masterModel = new ModelUIElement3D() { IsHitTestVisible = false };
        public Visual3D MasterModel => _masterModel;
        [Reactive]
        public IObservable<ItemId> SelectedItemIdChanges { get; set; }
        [Reactive]
        public IObservable<Rect3D> LocationChanges { get; set; }
        public CursorManager(IModelCache modelCache)
        {
            var idChanges = this.WhenAnyObservable(vm => vm.SelectedItemIdChanges);


            modelCache.Get(idChanges)
            .Subscribe(x => { });

            modelCache.Get(idChanges)
                .CombineLatest(
                    this.WhenAnyObservable(vm => vm.LocationChanges),
                    (model, location) => { return new { model, location }; }
                )
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    if (_masterModel.Model != x.model)
                        _masterModel.Model = x.model;

                    moveModel(x.location);
                });
        }
        private void moveModel(Rect3D location)
        {
            var matrix = _masterModel.Transform.Value;

            matrix.OffsetX = location.X + location.SizeX / 2;
            matrix.OffsetY = location.Y + location.SizeY / 2;
            matrix.OffsetZ = location.Z;

            _masterModel.Transform = new MatrixTransform3D(matrix);
        }
    }
}
