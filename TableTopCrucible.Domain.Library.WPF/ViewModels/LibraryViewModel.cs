using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Input;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.WPF.Services;
using TableTopCrucible.Core.WPF.ViewModels;
using TableTopCrucible.Domain.Library.WPF.Pages;
using TableTopCrucible.Domain.Library.WPF.PageViewModels;
using TableTopCrucible.Domain.MapEditor.WPF.PageViewModels;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public class LibraryViewModel : DisposableReactiveObjectBase
    {
        public TabViewModel Tabs { get; }
        public LibraryViewModel(
            TabViewModel tabs,
            TabService tabService,
            DevTestPageViewModel devTest,
            ItemEditorPageViewModel itemEditor,
            FileSetupPageViewModel fileSetup,
            AppSettingsPageViewModel settings,
            IMapEditorPageVm mapEditor)
        {
            this.Tabs = tabs;
            tabService.Append(itemEditor, true);
            tabService.Append(fileSetup, false);
            tabService.Append(mapEditor, false);
            tabService.Append(settings, false);
#if DEBUG
            tabService.Append(devTest, false);
#endif
        }
    }
}
