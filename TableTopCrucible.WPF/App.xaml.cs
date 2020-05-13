using Microsoft.Extensions.DependencyInjection;

using System;
using System.Windows;

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
        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceProvider = this.CreateServiceProvider();

            new MainWindow()
            {
                DataContext = serviceProvider.GetRequiredService<MainViewModel>()
            }.Show();
            base.OnStartup(e);
        }
        private IServiceProvider CreateServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();

            // services
            services.AddSingleton<IItemService, ItemService>();
            services.AddSingleton<IUserDataService, UserDataService>();

            // viewModels
            services.AddScoped<MainViewModel>();
            services.AddScoped<ItemListViewModel>();
            services.AddScoped<ItemEditorViewModel>();

            // commands
            services.AddSingleton<CreateItemCommand>();

            return services.BuildServiceProvider();
        }
    }
}
