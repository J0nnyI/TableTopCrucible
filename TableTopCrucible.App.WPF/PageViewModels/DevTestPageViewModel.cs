using DynamicData;
using DynamicData.Binding;

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
using TableTopCrucible.Domain.MapEditor.Core.Services;
using TableTopCrucible.Data.Models.Sources;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.App.WPF.PageViewModels
{
    public class DevTestPageViewModel : PageViewModelBase
    {
        public ICommand ThrowExCmd { get; } = new RelayCommand(_ => throw new Exception("crash it baby"));
        public ObservableCollectionExtended<Map> Maps { get; } = new ObservableCollectionExtended<Map>();

        public ObservableCollectionExtended<Floor> Floors { get; } = new ObservableCollectionExtended<Floor>();
        public ObservableCollectionExtended<TileLocation> Locations { get; } = new ObservableCollectionExtended<TileLocation>();
        public ObservableCollectionExtended<Model3DGroup> Cache { get; } = new ObservableCollectionExtended<Model3DGroup>();
        public DevTestPageViewModel(
            CreateThumbnailsCommand createAllThumbnails,
            NotificationCenterViewModel notificationCenterViewModel,
            CreateThumbnailsCommand generateThumbnails,
            IFloorDataService floorDataService,
            ITileLocationDataService tileLocationDataService,
            IMapDataService mapDataService,
            IModelCache modelCache
            ) : base("dev test", PackIconKind.DeveloperBoard)
        {
            CreateAllThumbnails = createAllThumbnails;
            NotificationCenterViewModel = notificationCenterViewModel;
            GenerateThumbnails = generateThumbnails;


            mapDataService.Get().Connect().ObserveOn(RxApp.MainThreadScheduler).Bind(Maps).Subscribe();
            floorDataService.Get().Connect().ObserveOn(RxApp.MainThreadScheduler).Bind(Floors).Subscribe();
            tileLocationDataService.Get().Connect().ObserveOn(RxApp.MainThreadScheduler).Bind(Locations).Subscribe();
            modelCache.Get().Connect().ObserveOn(RxApp.MainThreadScheduler).Bind(Cache).Subscribe();
        }


        public CreateThumbnailsCommand CreateAllThumbnails { get; }
        public NotificationCenterViewModel NotificationCenterViewModel { get; }
        public CreateThumbnailsCommand GenerateThumbnails { get; }
    }
}
