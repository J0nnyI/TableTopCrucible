using DynamicData;
using DynamicData.Binding;

using HelixToolkit.Wpf;

using Microsoft.AspNetCore.Identity;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Media3D;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Data.MapEditor.Models.Sources;
using TableTopCrucible.Data.MapEditor.Stores;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.MapEditor.Core.Managers;
using TableTopCrucible.Domain.MapEditor.Core.Models;
using TableTopCrucible.Domain.MapEditor.Core.Models.Views;
using TableTopCrucible.Domain.MapEditor.Core.Services;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

using static TableTopCrucible.Domain.MapEditor.Core.Managers.TileManager;

namespace TableTopCrucible.Domain.MapEditor.Core.Layers
{
    public interface ITileLayer
    {
        Visual3D MasterModel { get; }
        public FloorId FloorId { get; set; }
    }
    public class TileLayer : DisposableReactiveObjectBase, ITileLayer
    {
        private ContainerUIElement3D _masterModel = new ContainerUIElement3D();
        public Visual3D MasterModel => _masterModel;
        [Reactive]
        public FloorId FloorId { get; set; }
        private Subject<TileManagerMouseArgs> _tileMouseDown = new Subject<TileManagerMouseArgs>();
        private readonly ISelectionManager _selectionManager;

        public IObservable<TileManagerMouseArgs> TileMouseDown { get; }

        public TileLayer(
            ITileLocationDataService tileLocationDataService,
            IItemDataService itemService,
            IMapEditorManagementService mapEditorManagementService,
            IModelCache modelCache,
            ISelectionManager selectionManager)
        {
            TileMouseDown = _tileMouseDown.AsObservable();
            _selectionManager = selectionManager;
            var managers = new Dictionary<TileLocationId, TileManager>();
            var visuals = modelCache
                .Get()
                .Connect()
                .ObserveOn(RxApp.TaskpoolScheduler)
                .InnerJoinMany(
                    mapEditorManagementService
                        .GetLocationEx()
                        .Filter(this.WhenAnyValue(vm => vm.FloorId).ToFilter((TileLocationEx location, FloorId floorId) => location.FloorId == floorId)),
                    location => location.ItemId,
                    (model, locations) => { return new { model, locations }; })
                .ObserveOn(RxApp.MainThreadScheduler)
                .TransformMany(group => group.locations.Items.Select(location => { return new { location, group.model }; }), x => x.location.LocationId)
                .OnItemAdded(x =>
                {
                    var mgr = new TileManager(x.model, x.location, this._masterModel, this._selectionManager);
                    managers.Add(x.location.LocationId, mgr);
                })
                .OnItemRemoved(x =>
                {
                    var mgr = managers[x.location.LocationId];
                    mgr.Dispose();
                })
                .OnItemUpdated((current, previous) =>
                {
                    if (current.location.LocationId != previous.location.LocationId)
                        throw new InvalidOperationException($"TileLayer: current and previous location have different ids: {current.location.LocationId} / {previous.location.LocationId}");

                    var mgr = managers[current.location.LocationId];
                    mgr.HorizontalRotation = current.location.HorizontalRotation;
                    mgr.Location = current.location.Location.Location;

                })
                .TakeUntil(Destroy)
                .Subscribe();
        }

    }
}
