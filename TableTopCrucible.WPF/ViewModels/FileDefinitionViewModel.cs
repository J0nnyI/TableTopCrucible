using DynamicData;

using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Domain.Models.Views;
using TableTopCrucible.Domain.Services;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.WPF.ViewModels
{

    public class FileDefinitionViewModel : DisposableReactiveObjectBase
    {
        private readonly ReadOnlyObservableCollection<ExtendedFileInfo> _files;
        public ReadOnlyObservableCollection<ExtendedFileInfo> Files => _files;
        private readonly IFileInfoService _fileInfoService;

        public ICommand SynchronizeFiles { get; }
        public ICommand HashFiles { get; }
        public ICommand GenerateItems { get; }
        public FileDefinitionViewModel(IFileInfoService fileInfoService, GenerateItemsFromFilesCommand generateItems, SychronizeFilesCommand synchronizeFiles, HashFilesCommand hashFiles)
        {
            this.SynchronizeFiles = synchronizeFiles;
            this.HashFiles = hashFiles;
            this.GenerateItems = generateItems;

            this._fileInfoService = fileInfoService;
            this._fileInfoService
                .GetExtended()
                .Connect()
                .Bind(out _files)
                .TakeUntil(destroy)
                .Subscribe();
        }
    }
}
