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
        public TileLayer(ITileLocationDataService tileLocationDataService, IItemDataService itemService,IMapEditorManagementService mapEditorManagementService, IModelCache modelCache)
        {


            // todo: create the cache for all maps at one location to prevent redundant memory usage

            modelCache
                .Get()
                .Connect()
                .InnerJoin(
                    mapEditorManagementService
                        .GetLocationEx()
                        .Filter(this.WhenAnyValue(vm=>vm.FloorId).ToFilter((TileLocationEx location, FloorId floorId) => location.FloorId == floorId)),
                    location => location.ItemId,
                    (model, location) =>
                    {
                        var visual = new ModelVisual3D { Content = model };
                        visual.Move(location.Origin);
                        return visual;
                    })
                .Subscribe(change =>
                {
                    change.HanldeManyChanges(
                        adds => this.masterModel.Children.AddRange(adds.Select(adds => adds.Current)),
                        removes => this.masterModel.Children.RemoveMany(removes.Select(adds => adds.Current)),
                        updates =>
                        {
                            var changes = updates.Where(x => x.Current != x.Previous);

                            this.masterModel.Children.RemoveMany(
                                changes
                                    .Where(x => x.Previous.HasValue)
                                    .Select(x => x.Previous.Value));

                            this.masterModel.Children.AddRange(
                                changes.Select(x => x.Current));
                        });
                });


        }

        public void PlaceItem(ItemId item, Rect3D slot)
        {

        }
    }
}
