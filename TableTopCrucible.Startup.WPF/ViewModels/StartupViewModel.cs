using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Models.ValueTypes;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Core.WPF.Commands;
using TableTopCrucible.Core.WPF.Windows;
using TableTopCrucible.Data.SaveFile.Services;
using TableTopCrucible.Domain.Library.WPF.ViewModels;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Startup.WPF.ViewModels
{
    public class StartupViewModel : DisposableReactiveObjectBase
    {
        private readonly ISettingsService settingsService;
        private readonly ISaveService saveService;
        private readonly LibraryViewModel libraryViewModel;
        private readonly IInjectionProviderService injectionProvider;
        private readonly Window window;

        public IList<LibraryLocation> MostRecentLibraries { get; }
        public LibraryInfoService LibraryInfoService { get; }
        public ICommand OpenNewLibraryCommand { get; }
        public ICommand OpenExistingLibraryCommand { get; }
        public ICommand OpenListedLibraryCommand { get; }
        public Action<string> OpenExistingLibraryAction { get; }
        public StartupViewModel(
            ISettingsService settingsService,
            ISaveService saveService,
            OpenFileDialogCommand openExistingLibraryCommand,
            LibraryInfoService libraryInfoService,
            LibraryViewModel libraryViewModel,
            IInjectionProviderService injectionProvider,
            AppWindow window)
        {
            this.MostRecentLibraries = settingsService.MostRecentLibraries;
            this.OpenNewLibraryCommand = new RelayCommand(_ => openNewLibrary());
            this.OpenListedLibraryCommand = new RelayCommand(path => openExistingLibrary(path as string));
            this.settingsService = settingsService;
            this.saveService = saveService;
            this.OpenExistingLibraryCommand = openExistingLibraryCommand;
            OpenExistingLibraryAction = openExistingLibrary;
            LibraryInfoService = libraryInfoService;
            this.libraryViewModel = libraryViewModel;
            this.injectionProvider = injectionProvider;
            this.window = window;
        }
        private void openNewLibrary()
        {
            window.Title = "TableTopCrucible";
            window.Content = libraryViewModel;
        }
        private void openExistingLibrary(string path)
        {
            if (!File.Exists(path))
            {
                var details = "The file could not be found.";
                var actions = MessageBoxButton.OK;
                if (settingsService.MostRecentLibraries.Any(lib => lib.Path.ToLower() == path.ToLower()))
                {
                    details += "would you like to remove the library from the list?";
                    actions = MessageBoxButton.YesNo;
                }
                var result = MessageBox.Show("File not found", details, actions);
                if (result == MessageBoxResult.Yes)
                    this.settingsService.MostRecentLibraries.RemoveAll(lib => lib.Path.ToLower() == path.ToLower());
                return;
            }


            window.Title = "TableTopCrucible";
            window.Content = libraryViewModel;
            saveService.Load(path);

            if (settingsService.MostRecentLibraries == null)
                settingsService.MostRecentLibraries = new List<LibraryLocation>();

            settingsService.MostRecentLibraries.RemoveAll(library => library.Path == path);
            settingsService.MostRecentLibraries.Add(new LibraryLocation(false, path, DateTime.Now));
            settingsService.Save();
            window.WindowState = WindowState.Maximized;
        }
    }
}
