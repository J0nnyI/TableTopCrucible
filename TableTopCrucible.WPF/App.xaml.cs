using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Threading;

using TableTopCrucible.Core.Services;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Services;
using TableTopCrucible.WPF.Commands;
using TableTopCrucible.WPF.ViewModels;
using TableTopCrucible.WPF.Views;

namespace TableTopCrucible.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        public class InjectionProviderService : IInjectionProviderService, IDisposable
        {
            private readonly BehaviorSubject<IServiceProvider> _provider = new BehaviorSubject<IServiceProvider>(null);
            public ISubject<IServiceProvider> Provider => _provider;

            internal void SetProvider(IServiceProvider provider)
            {
                this._provider.OnNext(provider);
            }

            #region IDisposable Support
            private bool _disposedValue = false;
            protected virtual void dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                        _provider.Dispose();
                    }
                    _disposedValue = true;
                }
            }
            public void Dispose()
            {
                dispose(true);
            }
            #endregion
        }

        public class UiDispatcherService : IUiDispatcherService
        {
            public Dispatcher UiDispatcher => App.Current.Dispatcher;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceProvider = this._createServiceProvider();

            serviceProvider.GetRequiredService<IInjectionProviderService>()
                .Provider
                .OnNext(serviceProvider);


            var dss = serviceProvider.GetRequiredService<IDirectorySetupService>();
            dss.Patch(new DirectorySetupChangeset()
            {
                Path = @"F:\tmp\Folder A",
                Name = @"Folder A"
            });
            //dss.Patch(new DirectorySetupChangeset()
            //{
            //    Path = new Uri(@"F:\tmp\Folder B"),
            //    Name = @"Folder B"
            //});
            //var fileInfoService = serviceProvider.GetRequiredService<IFileInfoService>();
            //fileInfoService.Synchronize();

            this._disposables.Add(serviceProvider);

            new MainWindow()
            {
                DataContext = serviceProvider.GetRequiredService<MainViewModel>()
            }.Show();
            base.OnStartup(e);

        }
        private ServiceProvider _createServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();

            // base services
            services.AddSingleton<INotificationCenterService, NotificationCenterService>();
            // services
            services.AddSingleton<IItemService, ItemService>();
            services.AddSingleton<IInjectionProviderService, InjectionProviderService>();
            services.AddSingleton<IUiDispatcherService, UiDispatcherService>();
            services.AddSingleton<IItemTagService, ItemTagService>();
            services.AddSingleton<IFileInfoService, FileInfoService>();
            services.AddSingleton<IDirectorySetupService, DirectorySetupService>();

            // viewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<ItemListViewModel>();
            services.AddTransient<ItemCardViewModel>();
            services.AddTransient<TagEditorViewModel>();
            services.AddTransient<ItemEditorViewModel>();
            services.AddTransient<FileDefinitionViewModel>();
            services.AddTransient<DirectorySetupViewModel>();
            services.AddTransient<DirectorySetupCardViewModel>();
            services.AddTransient<NotificationCenterViewModel>();

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
            this._disposables.ForEach(x => x.Dispose());
        }
    }
}
