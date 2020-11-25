using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

using TableTopCrucible.Core.Services;
using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.WPF.Commands;
using TableTopCrucible.Core.WPF.Services;
using TableTopCrucible.Core.WPF.ViewModels;
using TableTopCrucible.Data.SaveFile.Services;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Library;
using TableTopCrucible.Domain.Library.WPF.Commands;
using TableTopCrucible.Domain.Library.WPF.Pages;
using TableTopCrucible.Domain.Library.WPF.PageViewModels;
using TableTopCrucible.Domain.Library.WPF.ViewModels;
using TableTopCrucible.WPF.Commands;
using TableTopCrucible.Domain.Library.WPF.Filter.ViewModel;
using TableTopCrucible.Startup.WPF.ViewModels;
using System.Windows.Threading;
using TableTopCrucible.Core.WPF.Windows;
using TableTopCrucible.Data.SaveFile.WPF.ViewModels;
using TableTopCrucible.Domain.Library.WPF.Tagging.ViewModels;
using TableTopCrucible.Data.MapEditor.Stores;
using TableTopCrucible.Domain.MapEditor.Core.Layers;
using TableTopCrucible.Domain.MapEditor.Core;
using TableTopCrucible.Domain.MapEditor.Core.Services;

namespace TableTopCrucible.App.WPF
{


    public partial class App : Application
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceProvider = this._createServiceProvider();
            this._disposables.Add(serviceProvider);

            var providerService = serviceProvider.GetRequiredService<IInjectionProviderService>() as InjectionProviderService;
            providerService.SetProvider(serviceProvider);

            var saveService = serviceProvider.GetRequiredService<ISaveService>();
            var tabService = serviceProvider.GetRequiredService<TabService>();

            if (e.Args.Length > 0 && File.Exists(e.Args[0]) && Path.GetExtension(e.Args[0]) == ".ttcl")
            {
                saveService.Load(e.Args[0]);
                tabService.SetTabIndex(1);
            }
            var window = serviceProvider.GetRequiredService<AppWindow>();
            window.Content = serviceProvider.GetRequiredService<StartupViewModel>();
            window.Show();
        }

        private void createCoreServices(IServiceCollection services)
        {
            services.AddSingleton<INotificationCenterService, NotificationCenterService>();
            services.AddSingleton<ISettingsService, Settings>();
            services.AddScoped<IInjectionProviderService, InjectionProviderService>();
            services.AddSingleton<ISaveService, SaveService>();
            services.AddSingleton<LibraryInfoService>();

        }
        private void createLibraryServices(IServiceCollection services)
        {
            // data services
            services.AddSingleton<IItemDataService, ItemService>();
            services.AddSingleton<IFileItemLinkService, FileItemLinkService>();
            services.AddSingleton<IFileDataService, FileDataService>();
            services.AddSingleton<IDirectoryDataService, DirectoryDataService>();
            // library domain services
            services.AddSingleton<ILibraryManagementService, LibraryManagementService>();
            services.AddSingleton<IThumbnailManagementService, ThumbnailManagementService>();
            services.AddSingleton<IFileManagementService, FileManagementService>();

            // WPF Services
            services.AddScoped<TabService>();

            // viewModels
            services.AddTransient<LibraryViewModel>();
            services.AddTransient<ItemListViewModel>();
            services.AddTransient<ItemCardViewModel>();
            services.AddTransient<IManualTagEditor, TagEditorViewModel>();
            services.AddTransient<IDrivenTagEditor, DrivenTagEditorViewModel>();
            services.AddTransient<ItemEditorViewModel>();
            services.AddTransient<IMultiItemEditor, MultiItemEditorViewModel>();
            services.AddTransient<FileListViewModel>();
            services.AddTransient<DirectoryListViewModel>();
            services.AddTransient<DirectorySetupCardViewModel>();
            services.AddTransient<NotificationCenterViewModel>();
            services.AddTransient<TabViewModel>();
            services.AddTransient<AppSettingsViewModel>();
            services.AddTransient<FileVersionListViewModel>();
            services.AddTransient<ItemListFilterViewModel>();
            services.AddTransient<IItemFilter, ItemFilterViewModel>();
            services.AddTransient<StartupViewModel>();
            services.AddTransient<SaveFileLoadingViewModel>();

            //   pages
            services.AddScoped<DevTestPageViewModel>();
            services.AddScoped<ItemEditorPageViewModel>();
            services.AddScoped<FileSetupPageViewModel>();
            services.AddScoped<AppSettingsPageViewModel>();

            // windows
            services.AddScoped<AppWindow>();

            // commands
            services.AddSingleton<CreateItemCommand>();
            services.AddSingleton<DeleteItemCommand>();
            services.AddSingleton<SaveItemCommand>();
            services.AddSingleton<CreateDirectorySetupCommand>();
            services.AddSingleton<SaveDirectorySetupCommand>();
            services.AddSingleton<DeleteDirectorySetupCommand>();
            services.AddSingleton<RemoveDirectorySetupRecursivelyCommand>();
            services.AddSingleton<GenerateItemsFromFilesCommand>();
            services.AddSingleton<SaveFileDialogCommand>();
            services.AddSingleton<OpenFileDialogCommand>();
            services.AddSingleton<OpenFileCommand>();
            services.AddSingleton<FileToClipboardCommand>();
            services.AddSingleton<CreateThumbnailsCommand>();
            services.AddSingleton<FullSyncCommand>();


        }
        private void createMapEditorServices(IServiceCollection services)
        {
            // data services
            services.AddSingleton<IFloorDataService, FloorDataService>();
            services.AddSingleton<IMapDataService, MapDataService>();
            services.AddSingleton<ITileLocationDataService, TileLocationDataService>();
            services.AddSingleton<IMapEditorManagementService, MapEditorManagementService>();
            // viewModels
            // helper
            services.AddTransient<IGridLayer, GridLayer>();
            services.AddTransient<IFloorManager, FloorManager>();
        }
        private ServiceProvider _createServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();
            createCoreServices(services);
            createLibraryServices(services);
            createMapEditorServices(services);
            return services.BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            this._disposables.Dispose();
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                File.WriteAllText(@"crashlog.txt", e.Exception.ToString());
            }
            catch
            {
            }

        }
    }
}
