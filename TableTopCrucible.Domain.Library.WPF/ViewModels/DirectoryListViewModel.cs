using DynamicData;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;

using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Services;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Library.WPF.Commands;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public class DirectoryListViewModel : DisposableReactiveObjectBase
    {
        public SynchronizeFilesCommand SynchronizeFiles { get; }
        public CreateDirectorySetupCommand CreateDirectorySetup { get; }
        public FullSyncCommand FullSync { get; }

        private ReadOnlyObservableCollection<DirectorySetupCardViewModel> _directories;
        public ReadOnlyObservableCollection<DirectorySetupCardViewModel> Directories => _directories;

        private IDirectoryDataService _directorySetupService;
        private IInjectionProviderService _injectionProviderService;
        public DirectoryListViewModel(
            IDirectoryDataService directorySetupService,
            IInjectionProviderService injectionProviderService,
            SynchronizeFilesCommand synchronizeFiles,
            CreateDirectorySetupCommand createDirectorySetup,
            FullSyncCommand fullSync)
        {
            this._directorySetupService = directorySetupService;
            this._injectionProviderService = injectionProviderService;
            this.SynchronizeFiles = synchronizeFiles;
            this.CreateDirectorySetup = createDirectorySetup;
            FullSync = fullSync;
            this._injectionProviderService
                .Provider
                .TakeUntil(destroy)
                .Subscribe(provider =>
                {

                    this._directorySetupService
                        .Get()
                        .Connect()
                        .Transform(item =>
                        {
                            var vm = provider.GetRequiredService<DirectorySetupCardViewModel>();
                            vm.DirectorySetupChanges.OnNext(item);
                            return vm;
                        })
                        .TakeUntil(destroy)
                        .DisposeMany()
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Bind(out _directories)
                        .Subscribe();
                });
        }
    }
}
