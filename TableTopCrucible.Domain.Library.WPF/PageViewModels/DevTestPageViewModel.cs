using DynamicData;

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
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.PageViewModels
{
    public class DevTestPageViewModel : PageViewModelBase
    {
        public ICommand ThrowExCmd { get; } = new RelayCommand(_ => throw new Exception("crash it baby"));
        public DevTestPageViewModel(
            CreateThumbnailsCommand createAllThumbnails,
            NotificationCenterViewModel notificationCenterViewModel,
            CreateThumbnailsCommand generateThumbnails
            ) : base("dev test", PackIconKind.DeveloperBoard)
        {
            CreateAllThumbnails = createAllThumbnails;
            NotificationCenterViewModel = notificationCenterViewModel;
            GenerateThumbnails = generateThumbnails;
        }


        public CreateThumbnailsCommand CreateAllThumbnails { get; }
        public NotificationCenterViewModel NotificationCenterViewModel { get; }
        public CreateThumbnailsCommand GenerateThumbnails { get; }
    }
}
