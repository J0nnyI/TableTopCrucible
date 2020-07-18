using DynamicData;

using ReactiveUI;

using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.Views;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{

    public class FileListViewModel : DisposableReactiveObjectBase
    {
        private readonly ReadOnlyObservableCollection<ExtendedFileInfo> _files;
        public ReadOnlyObservableCollection<ExtendedFileInfo> Files => _files;
        private readonly IFileDataService _fileInfoService;

        public ICommand SynchronizeFiles { get; }
        public ICommand HashFiles { get; }
        public ICommand GenerateItems { get; }
        public FileListViewModel(IFileDataService fileInfoService, GenerateItemsFromFilesCommand generateItems, SychronizeFilesCommand synchronizeFiles, HashFilesCommand hashFiles)
        {
            this.SynchronizeFiles = synchronizeFiles;
            this.HashFiles = hashFiles;
            this.GenerateItems = generateItems;

            this._fileInfoService = fileInfoService;
            this._fileInfoService
                .GetExtended()
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _files)
                .TakeUntil(destroy)
                .Subscribe();
        }
    }
}
