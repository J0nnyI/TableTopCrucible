using DynamicData;

using ReactiveUI;

using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.WPF.Commands;
using TableTopCrucible.Core.Helper;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{

    public class FileListViewModel : DisposableReactiveObjectBase
    {
        private readonly ReadOnlyObservableCollection<FileInfoEx> _files;
        public ReadOnlyObservableCollection<FileInfoEx> Files => _files;
        private readonly IFileDataService _fileInfoService;

        public ICommand GenerateItems { get; }
        public FileListViewModel(IFileDataService fileInfoService, GenerateItemsFromFilesCommand generateItems)
        {
            this.GenerateItems = generateItems;

            this._fileInfoService = fileInfoService;
            this._fileInfoService
                .GetExtended()
                .Connect()
                .Sort(file => file.AbsolutePath)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _files)
                .TakeUntil(destroy)
                .Subscribe();
        }
    }
}
