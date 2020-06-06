using DynamicData;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;

using TableTopCrucible.Domain.Models;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.Views;
using TableTopCrucible.Domain.Services;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.WPF.ViewModels
{
    public class FileDefinitionViewModel : DisposableReactiveObject
    {
        private readonly ReadOnlyObservableCollection<ExtendedFileInfo> _files;
        public ReadOnlyObservableCollection<ExtendedFileInfo> Files =>_files;
        private readonly IFileInfoService _fileInfoService;

        public SychronizeFilesCommand SynchronizeFiles { get; }

        public FileDefinitionViewModel(IFileInfoService fileInfoService, SychronizeFilesCommand synchronizeFiles)
        {
            this.SynchronizeFiles = synchronizeFiles;
            this._fileInfoService = fileInfoService;
            this._fileInfoService
                .GetFullFIleInfo()
                .Connect()
                .Bind(out _files)
                .TakeUntil(destroy)
                .Subscribe();
        }
    }
}
