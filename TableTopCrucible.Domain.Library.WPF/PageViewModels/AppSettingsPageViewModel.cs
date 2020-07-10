using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;
using TableTopCrucible.Core.WPF.PageViewModels;
using TableTopCrucible.Domain.Library.WPF.ViewModels;

namespace TableTopCrucible.Domain.Library.WPF.PageViewModels
{
    public class AppSettingsPageViewModel: PageViewModelBase
    {
        public AppSettingsViewModel Settings { get; }

        public AppSettingsPageViewModel(AppSettingsViewModel settings):base("Settings", PackIconKind.Settings)
        {
            this.Settings = settings;
        }
    }
}
