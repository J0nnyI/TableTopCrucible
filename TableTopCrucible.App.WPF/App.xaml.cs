﻿using Microsoft.Extensions.DependencyInjection;

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
using TableTopCrucible.Core.Utilities;
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

            new MainWindow()
            {
                DataContext = serviceProvider.GetRequiredService<MainViewModel>()
            }.Show();
        }
        private ServiceProvider _createServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();

            // core services
            services.AddSingleton<INotificationCenterService, NotificationCenterService>();
            services.AddSingleton<ISettingsService, Settings>();
            services.AddScoped<IInjectionProviderService, InjectionProviderService>();
            services.AddSingleton<ISaveService, SaveService>();
            // library domain services
            services.AddSingleton<IItemService, ItemService>();
            services.AddSingleton<IItemTagService, ItemTagService>();
            services.AddSingleton<IFileItemLinkService, FileItemLinkService>();
            services.AddSingleton<IFileDataService, FileDataService>();
            services.AddSingleton<IDirectoryDataService, DirectoryDataService>();
            services.AddSingleton<ILibraryManagementService, LibraryManagementService>();

            // WPF Services
            services.AddScoped<TabService>();

            // viewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<ItemListViewModel>();
            services.AddTransient<ItemCardViewModel>();
            services.AddTransient<TagEditorViewModel>();
            services.AddTransient<ItemEditorViewModel>();
            services.AddTransient<FileListViewModel>();
            services.AddTransient<DirectoryListViewModel>();
            services.AddTransient<DirectorySetupCardViewModel>();
            services.AddTransient<NotificationCenterViewModel>();
            services.AddTransient<TabViewModel>();
            services.AddTransient<AppSettingsViewModel>();
            services.AddTransient<FileVersionListViewModel>();
            services.AddTransient<MassThumbnailCreatorViewModel>();

            //   pages
            services.AddScoped<DevTestPageViewModel>();
            services.AddScoped<ItemEditorPageViewModel>();
            services.AddScoped<FileSetupPageViewModel>();
            services.AddScoped<AppSettingsPageViewModel>();

            // commands
            services.AddSingleton<CreateItemCommand>();
            services.AddSingleton<DeleteItemCommand>();
            services.AddSingleton<SaveItemCommand>();
            services.AddSingleton<CreateDirectorySetupCommand>();
            services.AddSingleton<SaveDirectorySetupCommand>();
            services.AddSingleton<DeleteDirectorySetupCommand>();
            services.AddSingleton<GenerateItemsFromFilesCommand>();
            services.AddSingleton<SaveFileDialogCommand>();
            services.AddSingleton<OpenFileDialogCommand>();
            services.AddSingleton<OpenFileCommand>();
            services.AddSingleton<FileToClipboardCommand>();
            services.AddSingleton<CreateAllThumbnailsCommand>();
            services.AddSingleton<FullSyncCommand>();


            return services.BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            this._disposables.Dispose();
        }
    }

}
