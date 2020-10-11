using DynamicData;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;

using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Core.Helper;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Library.WPF.Commands;
using TableTopCrucible.WPF.Commands;
using System.Windows.Input;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public class DirectoryListViewModel : DisposableReactiveObjectBase
    {
        public CreateDirectorySetupCommand CreateDirectorySetup { get; }
        public FullSyncCommand FullSync { get; }
        public ICommand GenerateItems { get; }

        private ReadOnlyObservableCollection<DirectorySetupCardViewModel> _directories;
        public ReadOnlyObservableCollection<DirectorySetupCardViewModel> Directories => _directories;

        private IDirectoryDataService _directorySetupService;
        private IInjectionProviderService _injectionProviderService;
        public DirectoryListViewModel(
            IDirectoryDataService directorySetupService,
            IInjectionProviderService injectionProviderService,
            CreateDirectorySetupCommand createDirectorySetup,
            FullSyncCommand fullSync,
            GenerateItemsFromFilesCommand generateItems)
        {
            this._directorySetupService = directorySetupService;
            this._injectionProviderService = injectionProviderService;
            this.CreateDirectorySetup = createDirectorySetup;
            FullSync = fullSync;
            GenerateItems = generateItems;
            this._injectionProviderService
                .Provider
                .Select(provider =>
                    this._directorySetupService
                        .Get()
                        .Connect()
                        .Sort(dir => (string)dir.Name)
                        .Transform(item =>
                        {
                            var vm = provider.GetRequiredService<DirectorySetupCardViewModel>();
                            vm.DirectorySetupChanges.OnNext(item);
                            return vm;
                        })
                )
                .Switch()
                .TakeUntil(destroy)
                .DisposeMany()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _directories)
                .Subscribe();
        }
    }
}
