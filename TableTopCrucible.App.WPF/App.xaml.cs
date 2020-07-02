using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;

using TableTopCrucible.Core.Services;
using TableTopCrucible.Core.WPF.Services;
using TableTopCrucible.Core.WPF.ViewModels;
using TableTopCrucible.Data.Services;
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
            saveService.Load("");

            new MainWindow()
            {
                DataContext = serviceProvider.GetRequiredService<MainViewModel>()
            }.Show();
        }
        private ServiceProvider _createServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();

            // base services
            services.AddSingleton<INotificationCenterService, NotificationCenterService>();
            // library domain services
            services.AddSingleton<IItemService, ItemService>();
            services.AddSingleton<IUiDispatcherService, UiDispatcherService>();
            services.AddSingleton<IItemTagService, ItemTagService>();
            services.AddSingleton<IFileDataService, FileDataService>();
            services.AddSingleton<IDirectoryDataService, DirectoryDataService>();
            services.AddSingleton<ISaveService, SaveService>();
            services.AddScoped<IInjectionProviderService, InjectionProviderService>();

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

            //   pages
            services.AddScoped<DevTestPageViewModel>();
            services.AddScoped<ItemEditorPageViewModel>();
            services.AddScoped<FileSetupPageViewModel>();

            // commands
            services.AddSingleton<CreateItemCommand>();
            services.AddSingleton<DeleteItemCommand>();
            services.AddSingleton<SaveItemCommand>();
            services.AddSingleton<SychronizeFilesCommand>();
            services.AddSingleton<CreateDirectorySetupCommand>();
            services.AddSingleton<SaveDirectorySetupCommand>();
            services.AddSingleton<DeleteDirectorySetupCommand>();
            services.AddSingleton<HashFilesCommand>();
            services.AddSingleton<GenerateItemsFromFilesCommand>();


            return services.BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            this._disposables.Dispose();
        }
    }

}
