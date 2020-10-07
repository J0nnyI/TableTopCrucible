using DynamicData;
using MaterialDesignThemes.Wpf;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Input;
using TableTopCrucible.Core.WPF.PageViewModels;
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
            CreateAllThumbnailsCommand createAllThumbnails
            ) : base("dev test", PackIconKind.DeveloperBoard)
        {
            CreateAllThumbnails = createAllThumbnails;
        }

        public CreateAllThumbnailsCommand CreateAllThumbnails { get; }
    }
}
