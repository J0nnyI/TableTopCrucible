using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Documents;
using TableTopCrucible.Domain.Services;
using TableTopCrucible.WPF.Commands;
using TableTopCrucible.WPF.ViewModels;

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

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceProvider = this._createServiceProvider();

            serviceProvider.GetRequiredService<IInjectionProviderService>()
                .Provider
                .OnNext(serviceProvider);

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

            // services
            services.AddSingleton<IItemService, ItemService>();
            services.AddSingleton<IUserDataService, UserDataService>();
            services.AddSingleton<IInjectionProviderService, InjectionProviderService>();
            services.AddSingleton<IItemTagService, ItemTagService>();
            // viewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<ItemListViewModel>();
            services.AddTransient<ItemEditorViewModel>();
            services.AddTransient<ItemCardViewModel>();

            // commands
            services.AddSingleton<CreateItemCommand>();

            return services.BuildServiceProvider();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            this._disposables.ForEach(x => x.Dispose());
        }
    }
}
