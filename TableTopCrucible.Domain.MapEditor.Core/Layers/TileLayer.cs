using DynamicData;
using DynamicData.Binding;

using HelixToolkit.Wpf;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Windows.Media.Media3D;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.MapEditor.Models.IDs;
using TableTopCrucible.Data.MapEditor.Models.Sources;
using TableTopCrucible.Data.MapEditor.Stores;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.MapEditor.Core.Models.Views;
using TableTopCrucible.Domain.MapEditor.Core.Services;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.MapEditor.Core.Layers
{
    public interface ITileLayer
    {
        Visual3D MasterModel { get; }
        public FloorId FloorId { get; set; }
    }
    public class TileLayer : DisposableReactiveObjectBase, ITileLayer
    {
        private ModelVisual3D masterModel = new ModelVisual3D();
        public Visual3D MasterModel => masterModel;
        [Reactive]
        public FloorId FloorId { get; set; }
        public TileLayer(ITileLocationDataService tileLocationDataService, IItemDataService itemService, IMapEditorManagementService mapEditorManagementService, IModelCache modelCache)
        {
            var visuals = modelCache
                .Get()
                .Connect()
                .InnerJoinMany(
                    mapEditorManagementService
                        .GetLocationEx()
                        .Filter(this.WhenAnyValue(vm => vm.FloorId).ToFilter((TileLocationEx location, FloorId floorId) => location.FloorId == floorId)),
                    location => location.ItemId,
                    (model, locations) => locations.Items.Select(location =>
                         {
                             var visual = new ModelVisual3D { Content = model };
                             var matrix = visual.Transform.Value;
                             //matrix.OffsetX=location.Origin.X;
                             //matrix.OffsetY=location.Origin.Y;
                             //matrix.OffsetZ=location.Origin.Z;
                             matrix.Rotate(new Quaternion(new Vector3D(0, 0, 1), location.HorizontalRotation));
                             visual.Transform = new MatrixTransform3D(matrix);
                             visual.Move(location.Origin);
                             return new { visual, location.LocationId };
                         })
                    )
                .TransformMany(visuals => visuals, visuals => visuals.LocationId)
                .Transform(x => x.visual)
                .OnItemAdded(visual => this.masterModel.Children.Add(visual))
                .OnItemRemoved(visual => this.masterModel.Children.Remove(visual))
                .OnItemUpdated((current, previous) =>
                {
                    if (current == previous)
                        return;
                    if (current != null)
                        this.masterModel.Children.Add(current);
                    if (previous != null)
                        this.masterModel.Children.Remove(previous);
                })
                .TakeUntil(destroy)
                .Subscribe();

        }
    }
}
