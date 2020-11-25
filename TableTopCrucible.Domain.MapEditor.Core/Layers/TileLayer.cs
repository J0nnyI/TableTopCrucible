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
        ModelVisual3D MasterModel { get; }
        public FloorId FloorId { get; set; }
    }
    public class TileLayer : DisposableReactiveObjectBase, ITileLayer
    {
        public ModelVisual3D MasterModel { get; } = new ModelVisual3D();
        [Reactive]
        public FloorId FloorId { get; set; }

        public TileLayer(ITileLocationDataService tileLocationDataService, IItemDataService itemService, IMapEditorManagementService mapEditorManagementService)
        {


            var modelImporter = new ModelImporter()
            {
                DefaultMaterial = Materials.LightGray
            };
            // todo: create the cache for all maps at one location to prevent redundant memory usage
            var modelCache = tileLocationDataService
                .Get()
                .Connect()
                .DistinctValues(m => m.ItemId)
                .InnerJoin(
                    itemService.GetExtended().Connect(),
                    item => item.ItemId,
                    (_, item) => item
                )
                .ObserveOn(RxApp.MainThreadScheduler)
                .Transform(item =>
                {
                    var model = modelImporter.Load(item.LatestFilePath);
                    model.PlaceAtOrigin();
                    return model;
                });

            modelCache
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
                        adds => this.MasterModel.Children.AddRange(adds.Select(adds => adds.Current)),
                        removes => this.MasterModel.Children.RemoveMany(removes.Select(adds => adds.Current)),
                        updates =>
                        {
                            var changes = updates.Where(x => x.Current != x.Previous);

                            this.MasterModel.Children.RemoveMany(
                                changes
                                    .Where(x => x.Previous.HasValue)
                                    .Select(x => x.Previous.Value));

                            this.MasterModel.Children.AddRange(
                                changes.Select(x => x.Current));
                        });
                });


        }

        public void PlaceItem(ItemId item, Rect3D slot)
        {

        }
    }
}
