using DynamicData;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Domain.Services;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.WPF.ViewModels
{
    public class DirectorySetupViewModel : DisposableReactiveObjectBase
    {
        public SychronizeFilesCommand SynchronizeFiles { get; }
        public CreateDirectorySetupCommand CreateDirectorySetup { get; }


        private ReadOnlyObservableCollection<DirectorySetupCardViewModel> _directories;
        public ReadOnlyObservableCollection<DirectorySetupCardViewModel> Directories => _directories;

        private IDirectorySetupService _directorySetupService;
        private IInjectionProviderService _injectionProviderService;
        public DirectorySetupViewModel(
            IDirectorySetupService directorySetupService,
            IInjectionProviderService injectionProviderService,
            SychronizeFilesCommand synchronizeFiles,
            CreateDirectorySetupCommand createDirectorySetup)
        {
            this._directorySetupService = directorySetupService;
            this._injectionProviderService = injectionProviderService;
            this.SynchronizeFiles = synchronizeFiles;
            this.CreateDirectorySetup = createDirectorySetup;

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
                        .Bind(out _directories)
                        .TakeUntil(destroy)
                        .Subscribe();
                });

        }
    }
}
